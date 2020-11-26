using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Runtime.Serialization;
using System.Threading.Tasks;

namespace RecipesApi.DTOs.Ingredients
{

    public class PushIngredientDto
    {
        [Required]
        public int Id { get; set; }
        [Required]
        public string Name { get; set; }
        [Required]
        public double Quantity { get; set; }
        [Required]
        public int Recipe_Id { get; set; }
        [Required]
        public int Unit_Id { get; set; } // TODO: Maybe keep Unit_ID ? So that as input, Unit is empty but Unit Id is provided, and as Output, we give both
    }
}
