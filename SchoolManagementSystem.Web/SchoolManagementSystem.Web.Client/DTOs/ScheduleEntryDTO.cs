using System;

namespace SchoolManagementSystem.Web.DTOs
{
    public class ScheduleEntryDTO
    {
        public int Id { get; set; }
        public int SchoolClassId { get; set; }
        public string ClassName { get; set; } = string.Empty;
        public int SubjectId { get; set; }
        public string SubjectName { get; set; } = string.Empty;
        public int? TeacherId { get; set; }
        public string TeacherName { get; set; } = string.Empty;
        public DayOfWeek DayOfWeek { get; set; }
        public TimeSpan StartTime { get; set; }
        public TimeSpan EndTime { get; set; }
        public string? RoomNumber { get; set; }
    }
}
