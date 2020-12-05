using RecipesApi.DTOs.Ingredients;
using RecipesApi.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace RecipesApi.DTOs.Recipes
{
    // TODO: should not have any timestamp. We update/control them on our side.
    public class PushRecipeDto
    {
        public int Id { get; set; }
        public string TitleShort { get; set; }
        public string TitleLong { get; set; }
        public string Description { get; set; }
        public string OriginalLink { get; set; }
        public string LastModifier { get; set; } // TODO: remove
        public DateTime? AuditDate { get; set; } // TODO: remove
        public DateTime? CreationDate { get; set; } // TODO: remove
        public IEnumerable<PushIngredientDto> Ingredients { get; set; }
        public IEnumerable<MediaDto> Media { get; set; }
        public IEnumerable<Instruction> Instructions { get; set; }
        public IEnumerable<Category> Categories { get; set; }
    }
}
