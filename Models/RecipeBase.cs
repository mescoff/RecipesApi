using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace RecipesApi.Models
{
    [Table("recipes")]

    public class RecipeBase
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Recipe_Id { get; set; }

        [Required]
        public string TitleShort { get; set; }
        public string TitleLong { get; set; }
        [Required]
        public string Description { get; set; }
        public string OriginalLink { get; set; }
        [Required]
        public string LastModifier { get; set; }

        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public DateTime? AuditDate { get; set; }
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public DateTime? CreationDate { get; set; }

       // [ForeignKey("RecipeIng_Id")]
        public virtual IEnumerable<Ingredient> Ingredients { get; set; }

    }
}
