using InternalTrainingSystem.Core.DTOs;
using InternalTrainingSystem.Core.Models;

namespace InternalTrainingSystem.Core.Repository.Interface
{
	public interface IAttendanceRepository
	{
        Task MarkAttendanceAsync(int scheduleId, List<AttendanceRequest> list);
        Task<bool> UpdateAttendanceAsync(int scheduleId, List<AttendanceRequest> list);
        Task<List<AttendanceResponse>> GetAttendanceByScheduleAsync(int scheduleId);
    }
}
