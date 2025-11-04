using InternalTrainingSystem.Core.DTOs;
using InternalTrainingSystem.Core.Repository.Implement;
using InternalTrainingSystem.Core.Repository.Interface;
using InternalTrainingSystem.Core.Services.Interface;

namespace InternalTrainingSystem.Core.Services.Implement
{
	public class StatisticService : IStatisticService
	{
		private readonly IStatisticRepository _statisticRepository;

		public StatisticService(IStatisticRepository statisticRepository)
		{
			_statisticRepository = statisticRepository;
		}

		public async Task<IEnumerable<CoursePassRateDto>> GetCoursePassRatesAsync()
		{
			return await _statisticRepository.GetCoursePassRatesAsync();
		}

		public async Task<IEnumerable<CoursePassRateDto>> GetHighestPassRateCourseAsync(int top)
		{
			return await _statisticRepository.GetHighestPassRateCourseAsync(top);
		}

		public async Task<IEnumerable<CoursePassRateDto>> GetLowestPassRateCourseAsync(int top)
		{
			return await _statisticRepository.GetLowestPassRateCourseAsync(top);
		}

		public async Task<double> GetOverallPassRateAsync()
		{
			return await _statisticRepository.GetOverallPassRateAsync();
		}

		public async Task<IEnumerable<StaffRejectStatisticDto>> GetTopStaffRejectingCoursesAsync(int top)
		{
			return await _statisticRepository.GetTopStaffRejectingCoursesAsync(top);
		}
	}
}
