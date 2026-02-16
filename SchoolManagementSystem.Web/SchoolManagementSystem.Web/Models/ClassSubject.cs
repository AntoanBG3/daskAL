using Microsoft.EntityFrameworkCore;

namespace SchoolManagementSystem.Web.Models
{
    [PrimaryKey(nameof(SchoolClassId), nameof(SubjectId))]
    public class ClassSubject
    {
        public int SchoolClassId { get; set; }
        public SchoolClass? SchoolClass { get; set; }

        public int SubjectId { get; set; }
        public Subject? Subject { get; set; }

        public ICollection<ScheduleEntry> ScheduleEntries { get; set; } = new List<ScheduleEntry>();
    }
}
