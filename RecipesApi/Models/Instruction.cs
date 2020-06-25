using Newtonsoft.Json;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace RecipesApi.Models
{
    public class Instruction : ICustomModel
    {
        [Key]
        [Column("RecipeInst_Id")]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        [Required]
        public int StepNum { get; set; }

        [Required]
        [MaxLength(500)]
        public string Description { get; set; }

        [Required]
        public int Recipe_Id { get; set; }

        [ForeignKey("Recipe_Id")]
        [JsonIgnore] // Ignore it to avoid cycle loop when querying Recipes. IMPORTANT
        public Recipe Recipe { get; set; }

        public override string ToString()
        {
            return JsonConvert.SerializeObject(this);
        }

    }
}
