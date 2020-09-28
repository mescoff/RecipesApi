using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace RecipesApi.Models
{
    [Table("recipe_timeintervals")]
    public class TimeInterval : ICustomModel
    {

        [Key]
        [Column("TimeInterval_Id")]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        [Required]
        [JsonIgnore]
        public int IntervalLabel_Id { get; set; }

        [ForeignKey("IntervalLabel_Id")]
        public TimeIntervalLabel Label { get; set; }

        public IEnumerable<TimeIntervalSpan> TimeSpans {get; set;}

    }
}
