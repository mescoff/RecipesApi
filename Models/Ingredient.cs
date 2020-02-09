using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace RecipesApi.Models
{
    [Table("recipe_ingredients")]
    public class Ingredient
    {
        [Key]
        public int RecipeIng_Id { get; set; }
        [Required]
        public string Name { get; set; }

        [Required]
        public double Quantity { get; set; }

        [Required]
        public int Recipe_Id { get; set; }

        [ForeignKey("Recipe_Id")]
        public virtual RecipeBase Recipe {get; set;}

        //public RecipeBase Recipe { get; set; }

        [Required]
        public int Unit_Id { get; set; }

        [ForeignKey("Unit_Id")]
        public virtual Unit Unit { get; set; }
        //public virtual Unit Unit { get; set; }
    }
}
