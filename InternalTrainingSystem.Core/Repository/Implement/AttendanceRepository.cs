using InternalTrainingSystem.Core.DB;
using InternalTrainingSystem.Core.Models;
using InternalTrainingSystem.Core.Repository.Interface;

namespace InternalTrainingSystem.Core.Repository.Implement
{
	public class AttendanceRepository : IAttendanceRepository
	{
		private readonly ApplicationDbContext _context;

		public AttendanceRepository(ApplicationDbContext context)
		{
			_context = context;
		}

		public Task AddAttendance(Attendance attendance)
		{
			throw new NotImplementedException();
		}
	}
}
