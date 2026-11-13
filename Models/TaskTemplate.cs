using System.ComponentModel.DataAnnotations;

namespace CustodialTaskList.Models
{
    public class TaskTemplate
    {
        public int Id { get; set; }

        [Required]
        [MaxLength(150)]
        public string TaskName { get; set; } = "";

        [Required]
        [MaxLength(20)]
        public string Shift { get; set; } = "Morning";

        // Monday = 1, Tuesday = 2, ... Friday = 5
        public int DayOfWeek { get; set; }

        public bool IsRepeating { get; set; } = true;

        public string RepeatFrequency { get; set; } = "Weekly";

        public string? WeekOccurrence { get; set; }

        public DateOnly StartDate { get; set; } = DateOnly.FromDateTime(DateTime.Today);

        public bool IsActive { get; set; } = true;
    }
}