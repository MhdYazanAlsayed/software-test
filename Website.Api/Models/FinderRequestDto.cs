using System.ComponentModel.DataAnnotations;

namespace Website.Models
{
    public class FinderRequestDto
    {
        [Required, Range(1, 9999)]
        public int StartYear { get; set; }

        [Required, Range(1, 9999)]
        public int EndYear { get; set; }

        [Required, Range(1, 31)]
        public int DayOfMonth { get; set; }

        [Required]
        public DayOfWeek TargetDayOfWeek { get; set; }
    }
}
