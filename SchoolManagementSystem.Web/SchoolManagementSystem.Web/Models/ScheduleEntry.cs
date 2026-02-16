using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SchoolManagementSystem.Web.Models
{
    public class ScheduleEntry
    {
        public int Id { get; set; }

        public int SchoolClassId { get; set; }
        public int SubjectId { get; set; }

        [ForeignKey($"{nameof(SchoolClassId)}, {nameof(SubjectId)}")]
        public ClassSubject? ClassSubject { get; set; }

        public DayOfWeek DayOfWeek { get; set; }
        public TimeSpan StartTime { get; set; }
        public TimeSpan EndTime { get; set; }
        public string? RoomNumber { get; set; }
    }
}
