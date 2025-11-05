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
		
	}
}
