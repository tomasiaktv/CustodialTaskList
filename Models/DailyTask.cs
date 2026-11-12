using System.ComponentModel.DataAnnotations;

namespace CustodialTaskList.Models
{
    public class DailyTask
    {
        public int Id { get; set; }

        public int TaskTemplateId { get; set; }

        [Required]
        [MaxLength(150)]
        public string TaskName { get; set; } = "";

        [Required]
        [MaxLength(20)]
        public string Shift { get; set; } = "Morning";

        public DateOnly ScheduledDate { get; set; }

        public bool IsCompleted { get; set; } = false;

        [MaxLength(4)]
        public string? CompletedByInitials { get; set; }

        public DateTimeOffset? CompletedAt { get; set; }
    }
}
