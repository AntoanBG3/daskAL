using SchoolManagementSystem.Web.DTOs;
using SchoolManagementSystem.Web.Models;

namespace SchoolManagementSystem.Web.Mappers
{
    public static class ScheduleEntryMapper
    {
        public static ScheduleEntryDTO ToDTO(ScheduleEntry entry)
        {
            return new ScheduleEntryDTO
            {
                Id = entry.Id,
                SchoolClassId = entry.SchoolClassId,
                ClassName = entry.ClassSubject?.SchoolClass?.Name ?? "Unknown Class",
                SubjectId = entry.SubjectId,
                SubjectName = entry.ClassSubject?.Subject?.Name ?? "Unknown Subject",
                TeacherId = entry.ClassSubject?.Subject?.TeacherId,
                TeacherName = entry.ClassSubject?.Subject?.Teacher != null
                    ? $"{entry.ClassSubject.Subject.Teacher.FirstName} {entry.ClassSubject.Subject.Teacher.LastName}"
                    : "No Teacher",
                DayOfWeek = entry.DayOfWeek,
                StartTime = entry.StartTime,
                EndTime = entry.EndTime,
                RoomNumber = entry.RoomNumber
            };
        }

        public static List<ScheduleEntryDTO> ToDTOList(IEnumerable<ScheduleEntry> entries)
        {
            return entries.Select(ToDTO).ToList();
        }
    }
}
