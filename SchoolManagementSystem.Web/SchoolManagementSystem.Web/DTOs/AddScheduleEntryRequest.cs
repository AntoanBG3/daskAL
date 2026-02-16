using System.ComponentModel.DataAnnotations;

namespace SchoolManagementSystem.Web.DTOs
{
    public class AddScheduleEntryRequest
    {
        public int SchoolClassId { get; set; }
        public int SubjectId { get; set; }
        public DayOfWeek DayOfWeek { get; set; }
        public TimeSpan StartTime { get; set; }
        public TimeSpan EndTime { get; set; }
        public string? RoomNumber { get; set; }
    }
}
