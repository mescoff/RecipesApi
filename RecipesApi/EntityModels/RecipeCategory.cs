﻿using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace RecipesApi.Models
{
    [Table("recipe_categories")]
    public class RecipeCategory : ICustomModel<RecipeCategory> //, IEquatable<RecipeCategory>
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
        //[JsonIgnore]
        public Category Category { get; set; }

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

        public bool Equals(RecipeCategory obj)
        {
            return (
                this.Id == obj.Id &&
                this.Recipe_Id == obj.Recipe_Id &&
                this.Category_Id == obj.Category_Id
                );
        }

        public override string ToString()
        {
            return JsonSerializer.Serialize(this);
        }

    }
}
