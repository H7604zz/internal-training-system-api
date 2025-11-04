using InternalTrainingSystem.Core.Services.Interface;
using Microsoft.AspNetCore.Mvc;

namespace InternalTrainingSystem.Core.Controllers
{
	[Route("api/[controller]")]
	[ApiController]
	public class StatisticController : ControllerBase
	{
		private readonly IStatisticService _statisticService;

		public StatisticController(IStatisticService statisticService)
		{
			_statisticService = statisticService;
		}
		[HttpGet("courses/pass-rate")]
		public async Task<IActionResult> GetCoursePassRates()
		{
			var data = await _statisticService.GetCoursePassRatesAsync();
			return Ok(data);
		}

		[HttpGet("courses/highest-pass")]
		public async Task<IActionResult> GetTopHighestPassCourses([FromQuery] int top = 5)
		{
			var data = await _statisticService.GetHighestPassRateCourseAsync(top);
			return Ok(data);
		}

		[HttpGet("courses/lowest-pass")]
		public async Task<IActionResult> GetTopLowestPassCourses([FromQuery] int top = 5)
		{
			var data = await _statisticService.GetLowestPassRateCourseAsync(top);
			return Ok(data);
		}

		[HttpGet("staff/top-rejects")]
		public async Task<IActionResult> GetTopRejectingStaff([FromQuery] int top = 5)
		{
			var data = await _statisticService.GetTopStaffRejectingCoursesAsync(top);
			return Ok(data);
		}

		[HttpGet("system/overall-pass-rate")]
		public async Task<IActionResult> GetOverallPassRate()
		{
			var rate = await _statisticService.GetOverallPassRateAsync();
			return Ok(new { OverallPassRate = rate });
		}
	}
}
