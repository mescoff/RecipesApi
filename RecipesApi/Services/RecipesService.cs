using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
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
            var result = this.Entities.Include(r => r.Media).Include(r => r.Ingredients).ThenInclude(i => i.Unit);
            return result;
        }

        public async override Task<Recipe> GetOne(int id)
        {
            var result = await this.Entities.Include(r => r.Media).Include(r => r.Ingredients).ThenInclude(i => i.Unit).SingleOrDefaultAsync(r => r.Id == id);
            return result;
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
                    var dbMedia = this.Context.Set<Media>().Find(id); // there should not be any doubt that it's there
                    this.Context.Set<Media>().Remove(dbMedia);
                    this._logger.LogInformation($"Deleting Ingredient with Id:{dbMedia.Id} and Name:{dbMedia.Title} on Recipe ID: {input.Id}");
                }
            }
        }
    }
}
