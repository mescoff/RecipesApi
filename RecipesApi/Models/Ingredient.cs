
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;
using System.Text.Json;
using System;

namespace RecipesApi.Models
{
    [Table("recipe_ingredients")]
    public class Ingredient: ICustomModel, IEquatable<Ingredient>
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
        [Range(1, int.MaxValue, ErrorMessage = "Ingredient needs a Unit Id")]
        public int Unit_Id { get; set; }

        [ForeignKey("Unit_Id")]
        public Unit Unit { get; set; }

        public override string ToString()
        {
            return JsonSerializer.Serialize(this);
        }


        public  bool Equals(Ingredient obj)
        {
            return (
                this.Id == obj.Id &&
                this.Name == obj.Name &&
                this.Quantity == obj.Quantity &&
                this.Recipe_Id == obj.Recipe_Id &&
                this.Unit_Id == obj.Unit_Id
            );
        }

        public override int GetHashCode()
        {
            return this.Name.GetHashCode() * this.Id;
        }
        //public virtual Unit Unit { get; set; }
    }
}
