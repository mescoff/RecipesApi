using Newtonsoft.Json;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace RecipesApi.Models
{
    public class RecipeCategory : ICustomModel
    {
        [Key]
        [Column("RecipeCat_Id")]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        [Required]
        public int Recipe_Id { get; set; }

        [ForeignKey("Recipe_Id")]
        [JsonIgnore] // Ignore it to avoid cycle loop when querying Recipes. IMPORTANT
        public Recipe Recipe { get; set; }

        [Required]
        public int Category_Id { get; set; }

        [ForeignKey("Category_Id")]
        public Category Category { get; set; }

        public override string ToString()
        {
            return JsonConvert.SerializeObject(this);
        }

    }
}
