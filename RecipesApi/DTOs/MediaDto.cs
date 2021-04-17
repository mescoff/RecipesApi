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

        //[Required]
        //[MinLength(1)]
        //public byte[] MediaBytes { get; set; }

        [Required]
        [MinLength(22)]
        // 22 char is min required for dataUrl representing image ? must have info in front and first char of image in base64. Ex: data:image/xx;base64,d
        // DataURL is in format "data:<MimeType>;base64,<imgBytesToBase64>   
        public string MediaDataUrl { get; set; }

        [Required]
        [MaxLength(200)]
        [RegularExpression("^[a-zA-Z0-9_]*$", ErrorMessage = "The Media title can only contain MAJ or MIN letters, digits, or _")]
        public string Title { get; set; }

        [MaxLength(50)]
        [RegularExpression("^[a-zA-Z0-9_]*$", ErrorMessage = "The Media tag can only contain MAJ or MIN letters, digits, or _")]

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
                this.MediaDataUrl.SequenceEqual(obj.MediaDataUrl) &&
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
                MediaDataUrl = obj.MediaDataUrl,
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
