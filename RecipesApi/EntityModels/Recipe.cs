using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace RecipesApi.Models
{
    [Table("recipes")]

    public class Recipe: RecipeBase   //, IEquatable<Recipe>
    {

        public Recipe()
        {
            this.Ingredients = new List<Ingredient>();
            this.Medias = new List<Media>();
            this.Instructions = new List<Instruction>();
            this.RecipeCategories = new List<RecipeCategory>();
        }

        //public Recipe(int Id, string TitleShort, string TitleLong, string Description, string OriginalLink, string LastModifier)

        [JsonIgnore]
        // To Fix inability of NET Json to deserialize property that's hidden in children class. : https://github.com/dotnet/runtime/issues/30964
        public IList<Ingredient> Ingredients { get; set; }

        [JsonIgnore] 
        // Why JsonIgnore: see Ingredients
        public IList<Media> Medias { get; set; }
        public IList<Instruction> Instructions { get; set; }
        public IList<RecipeCategory> RecipeCategories { get; set; }
        //public IEnumerable<TimeInterval> TimeIntervals { get; set; }
    }
}
