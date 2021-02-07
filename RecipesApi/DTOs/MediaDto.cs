using RecipesApi.Models;
using System;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text.Json;

namespace RecipesApi.DTOs
{
    public class MediaDto: IMedia, IEquatable<MediaDto>
    {
        public int Id { get; set; }

        [Required]
        [MinLength(1)]
        public byte[] MediaBytes { get; set; }

        [Required]
        [MaxLength(200)]
        [RegularExpression("[a-z-A-Z-0-9]", ErrorMessage = "Media title can only have regular characters (a to z) or digits")]
        public string Title { get; set; }

        [MaxLength(50)]
        [RegularExpression("[a-z-A-Z-0-9]", ErrorMessage = "Media tag can only have regular characters (a to z) or digits")]

        public string Tag { get; set; }

        [Required]
        [Range(1, int.MaxValue)]
        public int Recipe_Id { get; set; }

        public int RecipeInst_Id { get; set; }

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

        public bool Equals(MediaDto obj)
        {
            return (
                this.Id == obj.Id &&
                this.MediaBytes.SequenceEqual(obj.MediaBytes) &&
                this.Title == obj.Title &&
                this.Tag == obj.Tag &&
                this.Recipe_Id == obj.Recipe_Id
                );
        }

        public static MediaDto Copy(MediaDto obj)
        {
            return new MediaDto
            {
                Id = obj.Id,
                MediaBytes = obj.MediaBytes,
                Title = obj.Title,
                Tag = obj.Title,
                Recipe_Id = obj.Recipe_Id,
                RecipeInst_Id = obj.RecipeInst_Id
            };
        }

        public override string ToString()
        {
            return JsonSerializer.Serialize(this);
        }
    }
}
