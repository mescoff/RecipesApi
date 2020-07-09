
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;
using System.Text.Json;

namespace RecipesApi.Models
{
    [Table("recipe_ingredients")]
    public class Ingredient: ICustomModel
    {
        [Key]
        [Column("RecipeIng_Id")]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }
        [Required]
        public string Name { get; set; }

        [Required]
        public double Quantity { get; set; }

        [Required]
        public int Recipe_Id { get; set; }

        [ForeignKey("Recipe_Id")]
        [JsonIgnore] // Ignore it to avoid cycle loop when querying Recipes. IMPORTANT
        public Recipe Recipe {get; set;}
        //public virtual RecipeBase Recipe {get; set;}

        //public RecipeBase Recipe { get; set; }

        [Required]
        public int Unit_Id { get; set; }

        [ForeignKey("Unit_Id")]
        public Unit Unit { get; set; }

        public override string ToString()
        {
            return JsonSerializer.Serialize(this);
        }
        //public virtual Unit Unit { get; set; }
    }
}
