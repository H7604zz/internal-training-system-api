using InternalTrainingSystem.Core.Models;

namespace InternalTrainingSystem.Core.Repository.Interface
{
	public interface IAttendanceRepository
	{
		Task AddAttendance(Attendance attendance);
	}
}
