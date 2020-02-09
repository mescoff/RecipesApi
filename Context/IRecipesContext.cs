using Microsoft.EntityFrameworkCore;
using RecipesApi.Models;

namespace RecipesApi
{
    public interface IRecipesContext
    {
        DbSet<RecipeBase> Recipes { get; set; }
        DbSet<Unit> Units { get; set; }
        DbSet<Ingredient> Ingredients { get; set; }
    }
}