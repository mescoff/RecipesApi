using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace RecipesApi.Models
{
    [Table("timeinterval_labels")]
    public class TimeIntervalLabel
    {

        [Key]
        [Column("IntervalLabel_Id")]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        [Required]
        [MaxLength(100)]
        public string label { get; set; }
    }
}
