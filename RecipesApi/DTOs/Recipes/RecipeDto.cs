using RecipesApi.Models;
using System.Collections.Generic;

namespace RecipesApi.DTOs.Recipes
{

    /// <summary>
    /// Recipe class with overriden properties to allow better flexibility
    /// </summary>
    public class RecipeDto : RecipeBase //IEquatable<RecipeDto>
    {
        public RecipeDto()
        {
            this.Medias = new List<MediaDto>();
            this.Ingredients = new List<IngredientBase>();
            this.Instructions = new List<Instruction>();
            this.RecipeCategories = new List<RecipeCategory>();
        }

        public IList<MediaDto> Medias { get; set; }
        public IList<IngredientBase> Ingredients { get; set; } 

        public IList<Instruction> Instructions { get; set; }
        public IList<RecipeCategory> RecipeCategories { get; set; }
    }
}
