using RecipesApi.Models;
using System.Collections.Generic;

namespace RecipesApi
{
    public interface IRecipesService
    {
        IEnumerable<RecipeBase> GetAllRecipes();
    }
}