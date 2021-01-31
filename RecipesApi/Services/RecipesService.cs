using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using RecipesApi.Models;
using RecipesApi.Services;
using RecipesApi.Utils;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;

namespace RecipesApi
{
    /// <summary>
    /// Recipe service
    /// Handles relaying requests between controller and DB + perform logic when necessary
    /// </summary>
    public class RecipesService : EntityService<Recipe>  //IEntityService<RecipeBase>
    {
        protected readonly IMediaLogicHelper MediaHelper;

        public RecipesService(RecipesContext context, ILogger<RecipesService> logger, IMediaLogicHelper mediaHelper) : base(context, logger)
        {
        }

        public override IEnumerable<Recipe> GetAll()
        {
            var result = this.Entities.Include(r => r.Media).Include(r => r.RecipeCategories).ThenInclude(i => i.Category).Include(r => r.Instructions).Include(r => r.Ingredients).ThenInclude(i => i.Unit);
            return result;
        }

        public async override Task<Recipe> GetOne(int id)
        {
            var result = await this.Entities.Include(r => r.Media).Include(r => r.RecipeCategories).ThenInclude(i => i.Category).Include(r => r.Instructions).Include(r => r.Ingredients).ThenInclude(i => i.Unit).SingleOrDefaultAsync(r => r.Id == id);
            return result;
        }

        public async override Task<bool> UpdateOne(Recipe updatedRecipe)
        {
            // TODO: AsNoTracking is no longer needed. We changed default behavior to NoTracking in RecipeContext
            //var dbRecipe = this.Entities.Include(r => r.Media).Include(r => r.Ingredients).AsNoTracking().FirstOrDefault(r => r.Id == recipe.Id); // TODO: make async
            var dbRecipe = await this.GetOne(updatedRecipe.Id);

            int result = 0;
            if (dbRecipe != null)
            {         
                //Disconnected scenario. Below not needed we'll already have a new context ?                 
                // LAST UPDATE:
                // - KO: audit date doesn't update on DB side
    
                try
                {
                    //this.Entities.Attach(recipe);           
                    //// Handle all over updates
                    //this.Entities.Update(recipe);


                    // Update Ingredients
                    ScanAndApplyChanges<Ingredient>(updatedRecipe.Ingredients, dbRecipe.Ingredients);

                    // Update Instrutions
                    ScanAndApplyChanges<Instruction>(updatedRecipe.Instructions, dbRecipe.Instructions);

                    // Update Media
                    ScanMediaUpdates(updatedRecipe.Media, dbRecipe.Media);

                    // Update recipe itself
                    // TODO: (1) test with just update instead of below. (2) check if we shouldn't do below when updating related entities (rather than using update)
                    this.Context.Entry(dbRecipe).CurrentValues.SetValues(updatedRecipe);  // setting all values
                    this.Context.Entry(dbRecipe).State = EntityState.Modified;

                    var entries = this.Context.ChangeTracker.Entries(); // FOR DEBUGGING (add to watch to see what changes were detectdd on context side)
                    
                    result = this.Context.SaveChanges();
                }
                catch (Exception e)
                {
                    //this.Context.Entry(dbRecipe).State = EntityState.Unchanged;
                    var errorMessage = $"Error while updating entity. Rolling back changes: Entity={JsonConvert.SerializeObject(dbRecipe)} ErrorMessage ={e.InnerException?.Message}";
                    _logger.LogError(errorMessage);
                    return false;
                }

                return result > 0;
            }
            return false;
        }

        /// <summary>
        /// Cleanse the object of any unwanted parameters before pushing to DB
        /// </summary>
        /// <param name="recipe">The recipe</param>
        /// <returns></returns>
        protected override void prepareInputForCreateOrUpdate(Recipe input, bool isCreation)
        {

            // TODO: separate these back into post/put override
            if (isCreation)
            {
                //// CREATION
                // Make sure Id is 0 -- DB generates it
                input.Id = 0;

                if (input.Ingredients != null)
                {
                    foreach (var ingredient in input.Ingredients)
                    {
                        // clear Unid, recipeId and Id to be safe
                        ingredient.Id = 0;
                        ingredient.Recipe_Id = 0;
                        ingredient.Unit = null;  // TODO: Unit should also not be part of it ?
                        // TODO: there should be no Ingredient.Recipe
                    }
                }
                if (input.Media != null)
                {
                    foreach (var media in input.Media)
                    {
                        media.Id = 0;
                        media.Recipe_Id = 0;
                        media.Recipe = null;
                    }
                }

                // TODO: keep reseting other nested data here

            }
            else
            {
                //// UPDATE
                this._logger.LogInformation($"Updating Recipe w/ ID: {input.Id}");
                //input.AuditDate = DateTime.Now;
                // getting read only value of this recipe (thus the no tracking)
                // TODO: AsNoTracking is no longer needed. We changed default behavior to NoTracking in RecipeContext
                var dbRecipe = this.Entities.Include(r => r.Media).Include(r => r.Ingredients).AsNoTracking().FirstOrDefault(r => r.Id == input.Id); // TODO: make async

                ///// INGREDIENTS
                // Check for possible duplicate ingredients  
                var updatedRecipeIngredients = new HashSet<int>(input.Ingredients.Select(i => i.Id));
                if (updatedRecipeIngredients.Count() < input.Ingredients.Count())
                {
                    throw new DuplicateNameException($"You provided a recipe with duplicate ingredients ID"); // TODO: pick up throw in nicer way 
                }

                // Clear unit object if it was provided so it doesn't create new one TODO: handle this with AutoMapper ? (if separate RecipeDto)
                foreach (var ingredient in input.Ingredients)
                {
                    ingredient.Unit = null;
                }

                // Check for removed ingredients and delete them
                var dbRecipeIngredients = new HashSet<int>(dbRecipe.Ingredients.Select(i => i.Id));
                var ingMissingInUpdatedRecipe = dbRecipeIngredients.Where(i => !updatedRecipeIngredients.Contains(i)); // list of ingredients IDs missing from updated Recipe and need to remove
                foreach (var id in ingMissingInUpdatedRecipe)
                {
                    var dbIng = this.Context.Set<Ingredient>().Find(id); // there should not be any doubt that it's there
                    this.Context.Set<Ingredient>().Remove(dbIng);
                    this._logger.LogInformation($"Deleting Ingredient with Id:{dbIng.Id} and Name:{dbIng.Name} on Recipe ID: {input.Id}");
                }

                ///// MEDIA
                // Check for possible duplicate media  
                var updatedRecipeMedia = new HashSet<int>(input.Media.Select(i => i.Id));
                if (updatedRecipeMedia.Count() < input.Media.Count())
                {
                    throw new DuplicateNameException($"You provided a recipe with duplicate media ID"); // TODO: pick up throw in nicer way 
                }

                // Check for removed media and delete them
                var dbRecipeMedias = dbRecipe.Media == null ? null : new HashSet<int>(dbRecipe.Media.Select(i => i.Id));
                var ingMissingInUpdatedMedia = updatedRecipeMedia == null ? dbRecipeMedias : dbRecipeMedias.Where(i => !updatedRecipeMedia.Contains(i)); // list of ingredients IDs missing from updated Recipe and need to remove
                foreach (var id in ingMissingInUpdatedMedia)
                {
                    // TODO: Here use remove range: context.Children.RemoveRange(parent.Children)
                    // to avoid slow performance because of tracking
                    var dbMedia = this.Context.Set<Media>().Find(id); // there should not be any doubt that it's there
                    this.Context.Set<Media>().Remove(dbMedia);
                    this._logger.LogInformation($"Deleting Ingredient with Id:{dbMedia.Id} and Name:{dbMedia.Title} on Recipe ID: {input.Id}");
                }
            }
        }


        private void ScanMediaUpdates(IList<Media> updatedRecipeItems, IList<Media> existingRecipeItems)
        {
            var existingRecipeItemsIds = existingRecipeItems.Select(i => i.Id);  // items ids from DB recipe
            var updatedRecipeItemsIds = updatedRecipeItems.Select(i => i.Id);      // items ids from updated recipe

            // ADD ALL items if DB recipe doesn't have any yet
            if (!existingRecipeItemsIds.Any())
            {
                //
                //Context.AddRange(updatedRecipeItems);
            }
        }

        /// <summary>
        /// Scan all of the received/update recipe related entities/children and check for additions, removals and updates. And apply them
        /// </summary>
        /// <typeparam name="TEntity">Recipe's related entities (aka: Ingredients/Instructions/Media) </typeparam>
        /// <param name="updatedRecipeItems">The list of related entities on Received/Updated recipe</param>
        /// <param name="existingRecipeItems">The list of related entities on existing/DB recipe</param>
        private void ScanAndApplyChanges<TEntity>(IList<TEntity> updatedRecipeItems, IList<TEntity> existingRecipeItems) where TEntity : class, ICustomModel<TEntity>, new()
        {
            var existingRecipeItemsIds = existingRecipeItems.Select(i => i.Id);  // items ids from DB recipe
            var updatedRecipeItemsIds = updatedRecipeItems.Select(i => i.Id);      // items ids from updated recipe

            // ADD ALL items if DB recipe doesn't have any yet
            if (!existingRecipeItemsIds.Any())
            {
                // reset Ids to ensure DB will apply its own
                foreach(var item in updatedRecipeItems)
                {
                    item.Id = 0;
                }
                Context.AddRange(updatedRecipeItems);
            }
            else
            {
                var itemsIdsToDelete = existingRecipeItemsIds.Except(updatedRecipeItemsIds); // get items Ids in existing recipe missing from updated recipe
                var itemsIdsToAdd = updatedRecipeItemsIds.Except(existingRecipeItemsIds); // get items Ids in updated recipe missing from existing recipe

                // ADD items not present in DB recipe
                // TODO: Below should probably just look for items with Id=0. Client shouldn't send items with Id that doesn't exist yet.
                // TODO: Actually check what happens when providing recipe with Id = 75. Is it reset by DB or is it added as 75. ANd then we need to reset to 0 before saving to avoid wholes
                if (itemsIdsToAdd.Count() > 0)
                {
                    var ingredientsToAdd = updatedRecipeItems.Where(i => itemsIdsToAdd.Contains(i.Id));
                    // reset Ids to ensure DB will apply its own
                    foreach (var item in ingredientsToAdd)
                    {
                        item.Id = 0;
                    }
                    this.Context.Set<TEntity>().AddRange(ingredientsToAdd);
                }

                // DELETE items no longer in updated recipe
                if (itemsIdsToDelete.Count() > 0)
                {                         
                    // create new list of ingredients with just Id and delete (rather than using item from DB Recipe that is tracked)
                    var ingToDelete = existingRecipeItems.Where(i => itemsIdsToDelete.Contains(i.Id)).Select(i => new TEntity { Id = i.Id });
                    this.Context.Set<TEntity>().RemoveRange(ingToDelete);
                }

                // UPDATE matching items (between Existing and Updated recipe) if difference in properties is registered          
                // get ingredients IDs that are common to DB recipe and update.
                var itemsIdsInCommon = existingRecipeItemsIds.Intersect(updatedRecipeItemsIds);
                // and retrieve actual ingredients from existing ingredients
                var itemsInCommon = existingRecipeItems.Where(i => itemsIdsInCommon.Contains(i.Id));

                // Run through ingredients on updated recipe and DB recipe, If they contain the same info skip. Otherwise update
                foreach (var existingItem in itemsInCommon)
                {
                    //Ingredient receivedIng;
                    //updatedRecipeIngOrganized.TryGetValue(existingIng.Id, out receivedIng);
                    // locate item with same ID in updated recipe
                    var receivedItem = updatedRecipeItems.SingleOrDefault(i => i.Id == existingItem.Id);
                    if (receivedItem != null && !existingItem.Equals(receivedItem))
                    {
                        // TODO: GOOD TO KNOW: here we can use a DTO that's not translated if needed in update, as long as updated object has same properties as existing
                        //this.Context.Entry(existingIng).CurrentValues.SetValues(receivedIng);  // TODO: BUG values are not set to modified 
                        // var x = this.Context.Entry(existingIng).State;
                        //var entries = this.Context.ChangeTracker.Entries(); // FOR DEBUGGING (add to watch to see what changes were detectdd on context side)

                        // We override entire object. It's easier for now
                        this.Context.Set<TEntity>().Update(receivedItem);

                  }
                }

            }
        }

    }
}
