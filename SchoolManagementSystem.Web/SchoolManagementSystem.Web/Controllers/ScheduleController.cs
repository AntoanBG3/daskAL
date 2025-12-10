using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SchoolManagementSystem.Web.DTOs;
using SchoolManagementSystem.Web.Models;
using SchoolManagementSystem.Web.Services;

namespace SchoolManagementSystem.Web.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/[controller]")]
    public class ScheduleController : ControllerBase
    {
        private readonly IScheduleService _scheduleService;
        private readonly ILogger<ScheduleController> _logger;

        public ScheduleController(IScheduleService scheduleService, ILogger<ScheduleController> logger)
        {
            _scheduleService = scheduleService;
            _logger = logger;
        }

        [HttpPost]
        public async Task<IActionResult> AddScheduleEntry([FromBody] AddScheduleEntryRequest request)
        {
            if (request.StartTime >= request.EndTime)
            {
                return BadRequest("Start time must be before end time.");
            }

            try
            {
                await _scheduleService.AddScheduleEntryAsync(
                    request.SchoolClassId,
                    request.SubjectId,
                    request.DayOfWeek,
                    request.StartTime,
                    request.EndTime,
                    request.RoomNumber);

                return Ok("Schedule entry added successfully.");
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ex.Message);
            }
            catch (InvalidOperationException ex)
            {
                return Conflict(ex.Message); // 409 Conflict for scheduling conflicts
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error adding schedule entry.");
                return StatusCode(500, "An internal server error occurred.");
            }
        }

        [HttpGet("class/{schoolClassId}")]
        public async Task<IActionResult> GetScheduleForClass(int schoolClassId)
        {
            try
            {
                var schedule = await _scheduleService.GetScheduleForClassAsync(schoolClassId);
                var dtos = schedule.Select(ToDTO).ToList();
                return Ok(dtos);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching schedule for class {SchoolClassId}", schoolClassId);
                return StatusCode(500, "An internal server error occurred.");
            }
        }

        [HttpGet("teacher/{teacherId}")]
        public async Task<IActionResult> GetScheduleForTeacher(int teacherId)
        {
            try
            {
                var schedule = await _scheduleService.GetScheduleForTeacherAsync(teacherId);
                var dtos = schedule.Select(ToDTO).ToList();
                return Ok(dtos);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching schedule for teacher {TeacherId}", teacherId);
                return StatusCode(500, "An internal server error occurred.");
            }
        }

        private static ScheduleEntryDTO ToDTO(ScheduleEntry entry)
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
    }

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
