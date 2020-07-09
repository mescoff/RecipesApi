using RecipesApi.Models;
using System;
using System.Collections.Generic;

namespace RecipesApi.DTOs.Recipes
{
    public class GetRecipeDto
    {
        public string TitleShort { get; set; }
        public string TitleLong { get; set; }
        public string Description { get; set; }
        public string OriginalLink { get; set; }      
        public string LastModifier { get; set; }
        public DateTime? AuditDate { get; set; }
        public DateTime? CreationDate { get; set; }
        public IEnumerable<Ingredient> Ingredients { get; set; }
        public IEnumerable<Media> Media { get; set; }
        public IEnumerable<Instruction> Instructions { get; set; }
        public IEnumerable<GetCategoryDto> Categories { get; set; }
    }
}
