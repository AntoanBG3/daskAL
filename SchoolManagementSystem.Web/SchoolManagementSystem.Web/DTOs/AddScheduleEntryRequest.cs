using System.ComponentModel.DataAnnotations;

namespace SchoolManagementSystem.Web.DTOs
{
    public class AddScheduleEntryRequest
    {
        [Required]
        [Range(1, int.MaxValue, ErrorMessage = "SchoolClassId must be a valid ID")]
        public int SchoolClassId { get; set; }

        [Required]
        [Range(1, int.MaxValue, ErrorMessage = "SubjectId must be a valid ID")]
        public int SubjectId { get; set; }

        [Required]
        public DayOfWeek DayOfWeek { get; set; }
        public TimeSpan StartTime { get; set; }
        public TimeSpan EndTime { get; set; }
        public string? RoomNumber { get; set; }
    }
}
