using SchoolManagementSystem.Web.Models;

namespace SchoolManagementSystem.Web.Services
{
    public interface IScheduleService
    {
        Task AddScheduleEntryAsync(int schoolClassId, int subjectId, DayOfWeek dayOfWeek, TimeSpan startTime, TimeSpan endTime, string? roomNumber);
        Task<List<ScheduleEntry>> GetScheduleForClassAsync(int schoolClassId);
        Task<List<ScheduleEntry>> GetScheduleForTeacherAsync(int teacherId);
    }
}
