using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace RecipesApi.Models
{
    [Table("recipe_media")]
    public class Media: ICustomModel
    {
        [Key]
        [Column("RecipeMedia_Id")]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        [Required]
        [MaxLength(200)]
        public string MediaPath { get; set; } 

        [Required]
        [MaxLength(200)]
        public string Title { get; set; }

        [MaxLength(50)]
        public string Tag { get; set; }

        [ForeignKey("Recipe_Id")]
        [JsonIgnore] // Ignore it to avoid cycle loop when querying Recipes. IMPORTANT
        public Recipe Recipe { get; set; }

        [Required]
        public int Recipe_Id { get; set; }

        public override string ToString()
        {
            return JsonSerializer.Serialize(this);
        }
    }
}
