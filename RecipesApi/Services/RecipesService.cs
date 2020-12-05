using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using RecipesApi.Models;
using RecipesApi.Services;
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
      
        public RecipesService(RecipesContext context, ILogger<RecipesService> logger) : base(context, logger)
        {
        }

        public  override IEnumerable<Recipe> GetAll()
        {
            var result = this.Entities.Include(r => r.Media).Include(r => r.RecipeCategories).ThenInclude(i => i.Category).Include(r => r.Instructions).Include(r => r.Ingredients).ThenInclude(i => i.Unit);
            return result;
        }

        public async override Task<Recipe> GetOne(int id)
        {
            var result = await this.Entities.Include(r => r.Media).Include(r => r.RecipeCategories).ThenInclude(i => i.Category).Include(r => r.Instructions).Include(r => r.Ingredients).ThenInclude(i => i.Unit).SingleOrDefaultAsync(r => r.Id == id);
            return result;
        }

        public async override Task<bool> UpdateOne(Recipe recipe)
        {
            //var dbRecipe = await this.GetOne(recipe.Id);
            // FOR NOW just media and Ingredients. AS NOT TRACKING to avoid issues on removal
            // TODO: AsNoTracking is no longer needed. We changed default behavior to NoTracking in RecipeContext
            var dbRecipe = this.Entities.Include(r => r.Media).Include(r => r.Ingredients).AsNoTracking().FirstOrDefault(r => r.Id == recipe.Id); // TODO: make async

            //var dbRecipe = this.Entities.AsNoTracking().FirstOrDefault(r => r.Id == recipe.Id);
            //var dbRecipe = this.Entities.FirstOrDefault(r => r.Id == recipe.Id);
            var result = 0;
            if (dbRecipe != null)
            {
                // Recipe exists so we can update

                //HandleIngredientsUpdates(recipe, dbRecipe);

                //Disconnected scenario. Below not needed we'll already have a new context ?
                //using ( var newContext = new RecipesContext()){
                //    // This is not performant as it will act like recipe itself has been modified even if it hasn't. Use Attach instead
                //    // newContext.Set<Recipe>.Update(recipe);
                //    newContext.Set<Recipe>.Attach(recipe);
                //    newContext.SaveChanges();
                //}

                // UPDATE:
                // - new children are noticed on Attach
                // - BUT updates to object or children are not registered with just Attach...
                // - i will STILL have to delete manually children that were deleted (compare)
                // ==> FIND a generic way to easily run through every prop of each object and keep track of prop that aren't the same. Then set state to Modified for each of those

                // LAST UPDATE:
                // - Update works. But all children are updated even if no updates happened. For now it's okay but
                // later on we'll have to notice specific changes and only update these
                // - KO: audit date doesn't update on DB side
                // - Missing children aren't considered as Delete. FOR NOW THIS SHOULD BE DEFAULT BEHAVIOR. So we will manually compare
                // and remove children

                try
                {
                    //this.Entities.Attach(recipe);
                    //// Handle deletions
                    //HandleDeletedChildren(recipe, dbRecipe);

                    //// Handle all over updates
                    //this.Entities.Update(recipe);

                    HandleIngredientUpdates(recipe, dbRecipe);

                    //  this.Context.Entry(dbRecipe).State = EntityState.Modified;
                    //dbRecipe = input ;
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
                var dbRecipe = this.Entities.Include(r=> r.Media).Include(r => r.Ingredients).AsNoTracking().FirstOrDefault(r => r.Id == input.Id); // TODO: make async

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
                foreach(var id in ingMissingInUpdatedRecipe)
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


        // TODO: We should follow a disconnected model behavior !!! Get data as no tracking and then update it in new context
        private void HandleIngredientsUpdatesDeprecated(Recipe input, Recipe dbRecipe)
        {
            // Look for Deleted. Look for added. Look for duplicates

            ///// INGREDIENTS
            // Check for possible duplicate ingredients  
            var inputIngredientsId = new HashSet<int>(input.Ingredients.Select(i => i.Id));
            if (inputIngredientsId.Count() < input.Ingredients.Count())
            {
                throw new DuplicateNameException($"You provided a recipe with duplicate ingredients ID"); // TODO: pick up throw in nicer way 
            }

            // Clear unit object if it was provided so it doesn't create new one TODO: handle this with AutoMapper ? (if separate RecipeDto)
            // TODO: This will go away. We should only receive IngredientModel without Unit object
            //foreach (var ingredient in input.Ingredients)
            //{
            //    //ingredient.Unit = null;
            //}

            var dbRecipeIngredients = dbRecipe.Ingredients.ToList();

            // Check for removed ingredients and delete them
            var dbRecipeIngredientsId = new HashSet<int>(dbRecipeIngredients.Select(i => i.Id));
            var ingMissingInUpdatedRecipe = dbRecipeIngredientsId.Where(i => !inputIngredientsId.Contains(i)); // list of ingredients IDs missing from updated Recipe and need to remove
            // TODO: Use remove range here instead... Find all ing not dbRecipe.ing and remove that list
            foreach (var id in ingMissingInUpdatedRecipe)
            {
                var dbIng = this.Context.Set<Ingredient>().Find(id); // there should not be any doubt that it's there
                this.Context.Set<Ingredient>().Remove(dbIng);
                this._logger.LogInformation($"Deleting Ingredient with Id:{dbIng.Id} and Name:{dbIng.Name} on Recipe ID: {input.Id}");
            }

            // Check for added or updated ingredient  
            //var ingAdded = inputIngredientsId.Where(i => !dbRecipeIngredientsId.Contains(i));
            //foreach( var id in ingAdded)
            //{
            //    var ingredient = 
            //    this.Context.Set<Ingredient>().Remove(dbIng);
            //}
            foreach (var ing in input.Ingredients)
            {
                if (ing.Id == 0)
                {
                    // Overriding foreign key for peace of mind
                    ing.Recipe_Id = input.Id;
                    // Add ingredient
                    //this.Context.Set<Ingredient>().Add(ing);
                    this.Context.Set<Ingredient>().Add(ing);
                }
                else
                {
                    // find ingredient in DB recipe, if they're not equal override it
                    var dbIng = dbRecipeIngredients.Find(i => i.Id == ing.Id);
                    if (dbIng != null && !dbIng.Equals(ing))
                    {
                        //dbIng = ing;
                        // Still doesn't work.
                        // TODO: Think about detaching? Or override entire object? Or Update dbObject only properties that need to be updated... Through a method that goes generically through all props, and updates AuditTime



                        this.Context.Set<Ingredient>().Update(ing); // TODO: BUT THIS WILL set all values as modified... Also think to update auditDate
                        // https://stackoverflow.com/questions/46657813/how-to-update-record-using-entity-framework-core



                        //this.Context.Set<Ingredient>().Attach(ing);
                        //this.Context.Entry(ing).State = EntityState.Modified;
                        //dbRecipe.Ingredients.Attach().
                    }
                }
            }

        
            return;
        }


        private void HandleIngredientUpdates(Recipe input, Recipe dbRecipe)
        {
            var existingRecipeIngIds = dbRecipe.Ingredients.Select(i => i.Id);  // ingredients from DB recipe
            var updatedRecipeIngIds = input.Ingredients.Select(i => i.Id);      // ingredients from update received
            // retrieve ingredients in 0(1)
            var updatedRecipeIngOrganized = input.Ingredients.ToDictionary(i => i.Id, i => i);
            var existingRecipeIngOrganized = dbRecipe.Ingredients.ToDictionary(i => i.Id, i => i);

            // ADD ALL ingredients if DB recipe doesn't have any yet
            if (!existingRecipeIngIds.Any())
            {
                Context.AddRange(input.Ingredients);
            }
            else 
            {
                
                var ingredientsIdsToDelete = existingRecipeIngIds.Except(updatedRecipeIngIds); // get ing Ids in existing recipe missing from updated recipe
                var ingredientsIdsToAdd = updatedRecipeIngIds.Except(existingRecipeIngIds); // get ing Ids in updated recipe missing from existing recipe

                // DELETE
                //if (existingRecipeIngIds.Count() > updatedRecipeIngIds.Count())
                if (ingredientsIdsToDelete.Count() > 0)
                {
                    // if update contains less ingredients than in existing recipe. We find them and remove them.                 
                    var ingredientsToDelete = existingRecipeIngOrganized.Where(kp => ingredientsIdsToDelete.Contains(kp.Key)).Select(kp => kp.Value);
                    this.Context.Set<Ingredient>().RemoveRange(ingredientsToDelete);
                }
                // ADD
                if (ingredientsIdsToAdd.Count()> 0)
                //else if (existingRecipeIngIds.Count() < updatedRecipeIngIds.Count())
                {

                    var ingredientsToAdd = updatedRecipeIngOrganized.Where(kp => ingredientsIdsToAdd.Contains(kp.Key)).Select(kp => kp.Value);
                    this.Context.Set<Ingredient>().AddRange(ingredientsToAdd);
                }

                // UPDATE
                // get ingredients IDs that are common to DB recipe and update.
                var ingredientsIdsInCommon = existingRecipeIngIds.Intersect(updatedRecipeIngIds);
                // and retrieve actual ingredients from existing ingredients
                var ingredientsInCommon = existingRecipeIngOrganized.Where(kp => ingredientsIdsInCommon.Contains(kp.Key)).Select(kp => kp.Value); // TODO: is this unefficient? We use it in multiple places when we could also do recipe.Ingredients.Where( x.Contains(x.id))

                // Run through ingredients on updated recipe and DB recipe, If they contain the same info skip. Otherwise update
                foreach (var existingIng in ingredientsInCommon)
                {
                    Ingredient receivedIng;
                    updatedRecipeIngOrganized.TryGetValue(existingIng.Id, out receivedIng);
                    if (receivedIng != null && !existingIng.Equals(receivedIng))
                    {
                        // TODO: GOOD TO KNOW: here we can use a DTO that's not translated if needed in update, as long as updated object has same properties as existing
                        //this.Context.Entry(existingIng).CurrentValues.SetValues(receivedIng);  // TODO: BUG values are not set to modified 
                        // var x = this.Context.Entry(existingIng).State;
                        //var entries = this.Context.ChangeTracker.Entries(); // FOR DEBUGGING (add to watch to see what changes were detectdd on context side)

                        // making sure entity is detached (if only an ing update is ran it won't be attached, but if other ingredients were added/deleted before, this ingredient will be tracked)
                        var entryState = this.Context.Entry(input).State;
                        this.Context.Entry(input).State = EntityState.Detached;
                        this.Context.Ingredients.Update(receivedIng);

                        // TODO: Issue here is that when we only update ing, Update works without issue because it isn't tracked
                        // TODO: BUT if we performed other actions before (add/delete) then the whole recipe and ingredients are tracked and all we'd have to do is this.Context.Entry(existingIng).CurrentValues.SetValues(receivedIng) but we need to know if it's attached and right now i don't know how 
                    }
                }     
            }
        }

        private void HandleDeletedChildren(Recipe input, Recipe dbRecipe)
        {
            // TODO: Make sure that cascade delete is respected on DB side
            ////// INGREDIENTS
            //var dbRecipeIngredients = new HashSet<int>(dbRecipe.Ingredients.Select(i => i.Id));
            var updatedRecipeIng = input.Ingredients.Select(i => i.Id);
            // TODO: only do it if not same length of ingrdients?
            var ingMissingInUpdatedRecipe = dbRecipe.Ingredients.Where(i => !updatedRecipeIng.Contains(i.Id));
            //var ingMissingInUpdatedRecipeOLD = dbRecipeIngredients.Where(i => !input.Ingredients.Select( ing => ing.Id).Contains(i)).ToArray(); // list of ingredients IDs missing from updated Recipe and need to remove
            this.Context.Set<Ingredient>().RemoveRange(ingMissingInUpdatedRecipe);
            //this.Context.SaveChanges();
            //foreach (var id in ingMissingInUpdatedRecipe)
            //{
            //    var dbIng = this.Context.Set<Ingredient>().Find(id); // there should not be any doubt that it's there
            //    this.Context.Set<Ingredient>().Remove(dbIng);
            //    this._logger.LogInformation($"Deleting Ingredient with Id:{dbIng.Id} and Name:{dbIng.Name} on Recipe ID: {input.Id}");
            //}

            // Media
        }
    }
}
