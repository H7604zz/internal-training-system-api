using InternalTrainingSystem.Core.DTOs;

namespace InternalTrainingSystem.Core.Repository.Interface
{
	public interface IStatisticRepository
	{
		Task<IEnumerable<CoursePassRateDto>> GetCoursePassRatesAsync();
		Task<IEnumerable<CoursePassRateDto>> GetHighestPassRateCourseAsync(int top);
		Task<IEnumerable<CoursePassRateDto>> GetLowestPassRateCourseAsync(int top);
		Task<IEnumerable<StaffRejectStatisticDto>> GetTopStaffRejectingCoursesAsync(int top);
		Task<double> GetOverallPassRateAsync();
	}
}
