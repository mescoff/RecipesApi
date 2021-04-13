using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json;

namespace RecipesApi.Models
{
    [Table("units")]
    public class Unit : ICustomModel<Unit> //, IEquatable<Unit>
    {
        [Key]
        [Column("Unit_Id")]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }
        [Required]
        public string Name { get; set; }
        [Required]
        public string Symbol { get; set; }

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

        public bool Equals(Unit obj)
        {
            return (
                this.Id == obj.Id &&
                this.Name == obj.Name &&
                this.Symbol == obj.Symbol
                );
        }

        public override string ToString()
        {
            return JsonSerializer.Serialize(this);
        }
    }
}
