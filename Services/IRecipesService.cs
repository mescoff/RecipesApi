using RecipesApi.Models;
using System.Collections.Generic;

namespace RecipesApi
{
    public interface IRecipesService
    {
        IEnumerable<RecipeBase> GetAllRecipes();
        RecipeBase GetRecipe(int id);
        bool AddRecipe(RecipeBase recipe);
        bool DeleteRecipe(int id);
    }
}