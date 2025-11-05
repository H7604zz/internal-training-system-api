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
		[HttpGet("{courseId}/{quizId}")]
		public async Task<IActionResult> GetUserQuizHistory(int courseId, int quizId)
		{
			var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
			if (string.IsNullOrEmpty(userId))
				return Unauthorized();

			var result = await _courseHistoryService.GetUserQuizHistoryAsync(userId, courseId, quizId);
			return Ok(result);
		}
	}
}
