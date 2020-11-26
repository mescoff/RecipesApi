using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace RecipesApi.DTOs
{
    public class PushInstructionDto
    {
        [Required]
        public int Id { get; set; }

        [Required]
        public int StepNum { get; set; }

        [Required]
        [MaxLength(500)]
        public string Description { get; set; }

        [Required]
        public int Recipe_Id { get; set; }

        public int? RecipeMedia_Id { get; set; }
    }
}
