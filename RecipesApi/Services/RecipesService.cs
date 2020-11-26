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
      
        public RecipesService(DbContext context, ILogger<RecipesService> logger) : base(context, logger)
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
            var dbRecipe = await this.GetOne(recipe.Id);
            var result = 0;
            if (dbRecipe != null)
            {
                HandleIngredientsUpdates(recipe, dbRecipe);
                try
                {
                    this.Context.Entry(dbRecipe).State = EntityState.Modified;
                    //dbRecipe = input ;
                    var entries = this.Context.ChangeTracker.Entries();
                    result = this.Context.SaveChanges();
                }
                catch (Exception e)
                {
                    this.Context.Entry(dbRecipe).State = EntityState.Unchanged;
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
       
            if (isCreation)
            {
                //// CREATION
                // Clearing some properties for DB insert
                input.Id = 0;
             
                if (input.Ingredients != null)
                {
                    foreach (var ingredient in input.Ingredients)
                    {
                        // clear Unid, recipeId and Id to be safe
                        ingredient.Id = 0;
                        ingredient.Recipe_Id = 0;
                        ingredient.Unit = null;
                        // TODO: Shouldn't Recipe also be cleared?
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

            }
            else
            {
                //// UPDATE
                this._logger.LogInformation($"Updating Recipe w/ ID: {input.Id}");
                //input.AuditDate = DateTime.Now;
                // getting read only value of this recipe (thus the no tracking)
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


        private void HandleIngredientsUpdates(Recipe input, Recipe dbRecipe)
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
                    // TODO: verify that it has recipe ID ? (technically it's required at model level so should be OK) But override just in case?
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
    }
}
