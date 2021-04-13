using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json;

namespace RecipesApi.Models
{
    public class IngredientBase : ICustomModel<IngredientBase> //, IEquatable<IngredientBase>  // IEquatable<T> where T : class
    {
        public int Id { get; set; }

        [Required]
        [MaxLength(100, ErrorMessage = "Ingredient name cannot be longer than 100 characters")]
        public string Name { get; set; }

        [Range(0.1, double.MaxValue, ErrorMessage = "Please enter a quantity of 0.1 or greater")]
        public double Quantity { get; set; }

        [Range(1, int.MaxValue, ErrorMessage = "Ingredient needs a valid Recipe_Id")]
        public int Recipe_Id { get; set; }

        [Range(1, int.MaxValue, ErrorMessage = "Ingredient needs a valid Unit_Id")]
        public int Unit_Id { get; set; }

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

        public bool Equals(IngredientBase obj)
        {
            return (
                this.Id == obj.Id &&
                this.Name == obj.Name &&
                this.Quantity == obj.Quantity &&
                this.Recipe_Id == obj.Recipe_Id &&
                this.Unit_Id == obj.Unit_Id
            );
        }

        public override int GetHashCode()
        {
            return this.Name.GetHashCode() * this.Id;
        }
    }
}
