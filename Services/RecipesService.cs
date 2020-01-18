using Microsoft.Extensions.Logging;
using RecipesApi.Models;
using System.Collections.Generic;

namespace RecipesApi
{
    /// <summary>
    /// Recipe service
    /// Handles relaying requests between controller and DB + perform logic when necessary
    /// </summary>
    public class RecipesService: IRecipesService
    {
        private readonly ILogger _logger;
        private readonly RecipesContext _context;
        public RecipesService(ILogger<IRecipesService> logger, RecipesContext context)
        {
            this._logger = logger;
            this._context = context;
        }

        public IEnumerable<RecipeBase> GetAllRecipes()
        {
            return this._context.Recipes;
        }

        public RecipeBase GetRecipe(int id)
        {
            var recipe = this._context.Recipes.Find(id);
            return recipe;
        }

        //TODO: For now we are returning a bool but look into returning Concrete response with message and status code
        public bool AddRecipe(RecipeBase recipe)
        {
            // TODO: FIX. can't push null Dates
            var updatedRecipe = this.cleanRecipe(recipe);
            this._context.Recipes.Add(updatedRecipe);
            var result = this._context.SaveChanges();
            return result == 1 ;
        }

        public bool DeleteRecipe(int id)
        {
            this._context.Recipes.Remove(this._context.Recipes.Find(id));
            var result = this._context.SaveChanges();
            return result == 1;
        }

        /// <summary>
        /// Cleanse the object of any unwanted parameters before pushing to DB
        /// </summary>
        /// <param name="recipe">The recipe</param>
        /// <returns></returns>
        private RecipeBase cleanRecipe(RecipeBase recipe)
        {
            recipe.Recipe_Id = 0;
            recipe.AuditDate = null;
            recipe.CreationDate = null;
            return recipe;
        }
    }
}
