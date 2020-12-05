using RecipesApi.Validate;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json;

namespace RecipesApi.Models
{
    [Table("recipes")]

    public class Recipe: ICustomModel
    {

        public Recipe()
        {
            this.Ingredients = new List<Ingredient>();
            this.Media = new List<Media>();
            this.Instructions = new List<Instruction>();
            this.RecipeCategories = new List<RecipeCategory>();
        }

        [Key]
        [Column("Recipe_Id")]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)] // TODO: does not work if value is provided as expected. But see prettier way of just making id 0 on creation...
        public int Id { get; set; }

        [Required]
        [StringLength(50, MinimumLength = 5)]
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

        [DatabaseGenerated(DatabaseGeneratedOption.Computed)]
        public DateTime? AuditDate { get; set; }

        [DatabaseGenerated(DatabaseGeneratedOption.Computed)]
        public DateTime? CreationDate { get; set; }

        public IList<Ingredient> Ingredients { get; set; }
        public IList<Media> Media { get; set; }
        public IList<Instruction> Instructions { get; set; }
        public IList<RecipeCategory> RecipeCategories { get; set; }
        //public IEnumerable<TimeInterval> TimeIntervals { get; set; }
        public override string ToString()
        {
            return JsonSerializer.Serialize(this);
        }
    }
}
