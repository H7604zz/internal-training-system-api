using InternalTrainingSystem.Core.DTOs;

namespace InternalTrainingSystem.Core.Services.Interface
{
	public interface IStatisticService
	{
		Task<IEnumerable<CoursePassRateDto>> GetCoursePassRatesAsync();
		Task<IEnumerable<CoursePassRateDto>> GetHighestPassRateCourseAsync(int top);
		Task<IEnumerable<CoursePassRateDto>> GetLowestPassRateCourseAsync(int top);
		Task<IEnumerable<StaffRejectStatisticDto>> GetTopStaffRejectingCoursesAsync(int top);
		Task<double> GetOverallPassRateAsync();
	}
}
