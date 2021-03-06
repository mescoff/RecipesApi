﻿using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json;

namespace RecipesApi.Models
{
    public abstract class RecipeBase: ICustomModel<Recipe>
    {

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

        public override string ToString()
        {
            return JsonSerializer.Serialize(this);
        }

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
    }
}
