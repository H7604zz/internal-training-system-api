using InternalTrainingSystem.Core.Constants;
using InternalTrainingSystem.Core.Models;
using InternalTrainingSystem.Core.Services.Interface;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace InternalTrainingSystem.Core.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CourseEnrollmentController : ControllerBase
    {
        private readonly ICourseEnrollmentService _courseEnrollmentService;

        public CourseEnrollmentController(ICourseEnrollmentService courseEnrollmentService)
        {
            _courseEnrollmentService = courseEnrollmentService;
        }

        [HttpPost("confirm")]
        public IActionResult ConfirmEnrollment(int courseId, string userId, bool isConfirmed)
        {
            
            var enrollment = new CourseEnrollment
            {
                CourseId = courseId,
                UserId = userId,
                EnrollmentDate = DateTime.UtcNow,
                Status = isConfirmed
                    ? EnrollmentConstants.Status.Enrolled
                    : EnrollmentConstants.Status.Dropped,
                LastAccessedDate = DateTime.UtcNow
            };

            _courseEnrollmentService.AddCourseEnrollment(enrollment);

            return Ok(new
            {
                Message = isConfirmed
                    ? "Bạn đã xác nhận tham gia khóa học."
                    : "Bạn đã hủy tham gia khóa học."
            });
        }
    }
}
