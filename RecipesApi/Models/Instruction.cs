using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace RecipesApi.Models
{
    [Table("recipe_instructions")]
    public class Instruction : ICustomModel<Instruction> //, IEquatable<Instruction>
    {
        [Key]
        [Column("RecipeInst_Id")]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        //[Required] Not necessary since it will always default to 0 on creation and int cannot be null
        [Range(1, int.MaxValue, ErrorMessage = "Please enter a value bigger than 0")]
        public int StepNum { get; set; }

        [Required]
        [MaxLength(500, ErrorMessage = "Description cannot be longer than 500 characters")]
        public string Description { get; set; }

        //[Required] adding range is enough
        [Range(1, int.MaxValue, ErrorMessage = "Instruction needs a valid Recipe_Id")]
        public int Recipe_Id { get; set; }

        [ForeignKey("Recipe_Id")]
        [JsonIgnore] // Ignore it to avoid cycle loop when querying Recipes. IMPORTANT
        public Recipe Recipe { get; set; }

        public int? RecipeMedia_Id { get; set; }

        [ForeignKey("RecipeMedia_Id")]
        [JsonIgnore]
        public Media Media { get; set; } // TODO: This is probably broken

        public bool Equals(Instruction obj)
        {
            return (
                this.Id == obj.Id &&
                this.StepNum == obj.StepNum &&
                this.Description == obj.Description &&
                this.Recipe_Id == obj.Recipe_Id &&
                this.RecipeMedia_Id == obj.RecipeMedia_Id
             );
        }

        public override string ToString()
        {
            return JsonSerializer.Serialize(this);
        }

    }
}
