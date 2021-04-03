using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace RecipesApi.Models
{
    [Table("recipes")]

    public class Recipe : ICustomModel<Recipe> //, IEquatable<Recipe>
    {

        public Recipe()
        {
            this.Ingredients = new List<Ingredient>();
            this.Medias = new List<Media>();
            this.Instructions = new List<Instruction>();
            this.RecipeCategories = new List<RecipeCategory>();
        }

        [Key]
        [Column("Recipe_Id")]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)] // TODO: does not work if value is provided as expected. But see prettier way of just making id 0 on creation...
        public int Id { get; set; }

        [Required]
        [StringLength(50, MinimumLength = 5, ErrorMessage = "The Recipe's short Title length needs to be between 5 and 50 characters")]
        public string TitleShort { get; set; }

        [StringLength(150, ErrorMessage = "The Recipe's long Title length cannot exceed 150 characters")]
        public string TitleLong { get; set; }

        [Required]
        [StringLength(2000, ErrorMessage = "The Recipe's description length cannot exceed 2000 characters")]
        public string Description { get; set; }

        [StringLength(500, ErrorMessage = "The Recipe's original link length cannot exceed 500 characters")]
        [RegularExpression(@"^((([A-Za-z]{3,9}:(?:\/\/)?)(?:[-;:&=\+\$,\w]+@)?[A-Za-z0-9.-]+|(?:www.|[-;:&=\+\$,\w]+@)[A-Za-z0-9.-]+)((?:\/[\+~%\/.\w-_]*)?\??(?:[-\+=&;%@.\w_]*)#?(?:[\w]*))?)$",
            ErrorMessage = "Link provided is not a valid URL")]
        public string OriginalLink { get; set; }

        [Required]
        [StringLength(500, ErrorMessage = "The Recipe's last modifier length cannot exceed 2000 characters")]
        public string LastModifier { get; set; }

        [DatabaseGenerated(DatabaseGeneratedOption.Computed)]
        public DateTime? AuditDate { get; set; }

        [DatabaseGenerated(DatabaseGeneratedOption.Computed)]
        public DateTime? CreationDate { get; set; }

        [JsonIgnore]
        // To Fix inability of NET Json to deserialize property that's hidden in children class : https://github.com/dotnet/runtime/issues/30964
        public IList<Ingredient> Ingredients { get; set; }

        [JsonIgnore] 
        // Why: see Ingredients
        public IList<Media> Medias { get; set; }
        public IList<Instruction> Instructions { get; set; }
        public IList<RecipeCategory> RecipeCategories { get; set; }
        //public IEnumerable<TimeInterval> TimeIntervals { get; set; }

        public bool Equals(Recipe obj)
        {
            return (
                this.Id == obj.Id &&
                this.TitleShort == obj.TitleShort &&
                this.TitleLong == obj.TitleLong &&
                this.Description == obj.Description &&
                this.OriginalLink == obj.OriginalLink &&
                this.LastModifier == obj.LastModifier &&
                this.AuditDate == obj.AuditDate &&
                this.CreationDate == obj.CreationDate
                );
        }

        public override string ToString()
        {
            return JsonSerializer.Serialize(this);
        }
    }
}
