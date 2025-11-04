using InternalTrainingSystem.Core.Constants;
using InternalTrainingSystem.Core.DB;
using InternalTrainingSystem.Core.DTOs;
using InternalTrainingSystem.Core.Models;
using InternalTrainingSystem.Core.Repository.Interface;
using Microsoft.EntityFrameworkCore;

namespace InternalTrainingSystem.Core.Repository.Implement
{
	public class StatisticRepository : IStatisticRepository
	{
		private readonly ApplicationDbContext _context;

		public StatisticRepository(ApplicationDbContext context)
		{
			_context = context;
		}

		public async Task<IEnumerable<CoursePassRateDto>> GetCoursePassRatesAsync()
		{
			var result = await _context.Courses
								.Select(c => new CoursePassRateDto
								{
									CourseId = c.CourseId,
									CourseName = c.CourseName,
									TotalParticipants = c.CourseEnrollments
												.Count(e => e.Status != EnrollmentConstants.Status.NotEnrolled),
									TotalPassed = c.CourseEnrollments
												.Count(e => e.Status == EnrollmentConstants.Status.Completed)
								})
								.ToListAsync();

			foreach (var item in result)
			{
				item.PassRate = item.TotalParticipants == 0
						? 0
						: Math.Round((double)item.TotalPassed / item.TotalParticipants * 100, 2);
			}

			return result;
		}

		public async Task<IEnumerable<CoursePassRateDto>> GetHighestPassRateCourseAsync(int top)
		{
			var courseRates = await GetCoursePassRatesAsync();
			return courseRates
					.OrderByDescending(c => c.PassRate)
					.ThenByDescending(c => c.TotalParticipants)
					.Take(top)
					.ToList();
		}

		public async Task<IEnumerable<CoursePassRateDto>> GetLowestPassRateCourseAsync(int top)
		{
			var courseRates = await GetCoursePassRatesAsync();
			return courseRates
					.OrderBy(c => c.PassRate)
					.ThenBy(c => c.TotalParticipants)
					.Take(top)
					.ToList();
		}

		public async Task<IEnumerable<StaffRejectStatisticDto>> GetTopStaffRejectingCoursesAsync(int top)
		{
			var result = await _context.Set<CourseEnrollment>()
							 .Where(e => e.Status == "Rejected")
							 .GroupBy(e => new { e.UserId, e.User.FullName })
							 .Select(g => new StaffRejectStatisticDto
							 {
								 StaffId = g.Key.UserId,
								 StaffName = g.Key.FullName,
								 RejectCount = g.Count()
							 })
							 .OrderByDescending(x => x.RejectCount)
							 .Take(top)
							 .ToListAsync();

			return result;
		}

		public async Task<double> GetOverallPassRateAsync()
		{
			var enrollments = await _context.Set<CourseEnrollment>()
					.Where(e => e.Status != EnrollmentConstants.Status.NotEnrolled)
					.ToListAsync();

			if (!enrollments.Any()) return 0;

			var totalPassed = enrollments.Count(e => e.Status == EnrollmentConstants.Status.Completed);
			return Math.Round((double)totalPassed / enrollments.Count * 100, 2);
		}
	}
}
