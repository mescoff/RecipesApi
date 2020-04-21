using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace RecipesApi.Models
{
    [Table("recipes")]

    public class Recipe: ICustomModel
    {
        [Key]
        [Column("Recipe_Id")]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        [Required]
        [StringLength(50, MinimumLength = 10)]
        public string TitleShort { get; set; }

        [StringLength(150)]
        public string TitleLong { get; set; }

        [Required]
        [StringLength(2000)]
        public string Description { get; set; }

        [StringLength(500)]
        public string OriginalLink { get; set; }

        [Required]
        [StringLength(500)]
        public string LastModifier { get; set; }

        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public DateTime? AuditDate { get; set; }
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public DateTime? CreationDate { get; set; }

        public IEnumerable<Ingredient> Ingredients { get; set; }
    }
}
