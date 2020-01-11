using System;
using System.ComponentModel.DataAnnotations;

namespace RecipesApi.Models
{
    public class RecipeBase
    {
       [Required]
        public int RecipeId { get; set; }

        [Required]
        public string TitleShort { get; set; }
        public string TitleLong { get; set; }
        public string Description { get; set; }
        public string OriginalLink { get; set; }
        [Required]
        public string LastModifier { get; set; }
        public DateTime AuditDate { get; set; }
        public DateTime CreationDate { get; set; }

    }
}
