using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace RecipesApi.Models
{
    [Table("recipe_timeintervalspans")]
    public class TimeIntervalSpan
    {
        [Key]
        [Column("IntervalSpan_Id")]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        [Required]
        public int TimeValue { get; set; }

        /// <summary>
        /// Time Unit (should be Hours, Minutes or Seconds)
        /// </summary>
        [Required]
        public string TimeUnit { get; set; }

        [Required]
        public int TimeInterval_Id { get; set; }

        [ForeignKey("TimeInterval_Id")]
        [JsonIgnore] // Ignore it to avoid cycle loop when querying Recipes. IMPORTANT
        public TimeInterval TimeInterval { get; set; }
    }
}
