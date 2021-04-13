using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace RecipesApi.Models
{
    [Table("recipe_medias")]
    public class Media : IMedia, IEquatable<Media>
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
        [RegularExpression("[a-z-A-Z-0-9]", ErrorMessage = "Media title can only have regular characters (a to z) or digits")]

        public string Title { get; set; }

        [MaxLength(50)]
        [RegularExpression("[a-z-A-Z-0-9]", ErrorMessage = "Media Tag can only have regular characters (a to z) or digits")]

        public string Tag { get; set; }

        [ForeignKey("Recipe_Id")]
        [JsonIgnore] // Ignore it to avoid cycle loop when querying Recipes. IMPORTANT
        public Recipe Recipe { get; set; }

        [Required]
        [Range(1, int.MaxValue)]
        public int Recipe_Id { get; set; }

        /// <summary>
        /// Adding a way to retrieve property through reflection
        /// </summary>
        /// <param name="propertyName">Property Name</param>
        /// <returns>Value of property on Get. Otherwise nothing</returns>
        public object this[string propertyName]
        {
            get { return this.GetType().GetProperty(propertyName).GetValue(this, null); }
            set { this.GetType().GetProperty(propertyName).SetValue(this, value, null); }
        }

        public bool Equals(Media obj)
        {
            return (
                this.Id == obj.Id &&
                this.MediaPath == obj.MediaPath &&
                this.Title == obj.Title &&
                this.Tag == obj.Tag &&
                this.Recipe_Id == obj.Recipe_Id
                );
        }

        public override string ToString()
        {
            return JsonSerializer.Serialize(this);
        }
    }
}
