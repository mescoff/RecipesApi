using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace RecipesApi.Models
{
    [Table("recipe_timeintervals")]
    public class TimeInterval : ICustomModel<TimeInterval> //, IEquatable<TimeInterval>
    {
        public TimeInterval()
        {
            this.TimeSpans = new List<TimeIntervalSpan>();
        }

        [Key]
        [Column("TimeInterval_Id")]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        [Required]
        [JsonIgnore]
        public int IntervalLabel_Id { get; set; }

        [ForeignKey("IntervalLabel_Id")]
        public TimeIntervalLabel Label { get; set; }

        public IEnumerable<TimeIntervalSpan> TimeSpans { get; set; }

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

        public bool Equals(TimeInterval obj)
        {
            return (
                this.Id == obj.Id &&
                this.IntervalLabel_Id == obj.IntervalLabel_Id &&
                this.Label == obj.Label
                );
        }
        public override string ToString()
        {
            return JsonSerializer.Serialize(this);
        }

    }
}
