using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SchoolManagementSystem.Web.DTOs;
using SchoolManagementSystem.Web.Mappers;
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
        [Authorize(Roles = "Admin,Teacher")]
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
                var dtos = schedule.Select(ScheduleEntryMapper.ToDTO).ToList();
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
                var dtos = schedule.Select(ScheduleEntryMapper.ToDTO).ToList();
                return Ok(dtos);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching schedule for teacher {TeacherId}", teacherId);
                return StatusCode(500, "An internal server error occurred.");
            }
        }
    }
}
