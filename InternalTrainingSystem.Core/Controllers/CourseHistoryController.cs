using InternalTrainingSystem.Core.Services.Interface;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace InternalTrainingSystem.Core.Controllers
{
	[Route("api/[controller]")]
	[ApiController]
	public class CourseHistoryController : ControllerBase
	{
		private readonly ICourseHistoryService _courseHistoryService;

		public CourseHistoryController(ICourseHistoryService courseHistoryService)
		{
			_courseHistoryService = courseHistoryService;
		}
		[HttpGet("{courseId}")]
		public async Task<IActionResult> GetUserCourseHistory(int courseId)
		{
			var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
			if (string.IsNullOrEmpty(userId))
				return Unauthorized();
			var result = await _courseHistoryService.GetUserQuizHistoryAsync(userId, courseId);
			return Ok(result);
		}
	}
}
