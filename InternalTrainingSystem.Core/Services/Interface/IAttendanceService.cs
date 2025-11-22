using InternalTrainingSystem.Core.DTOs;

namespace InternalTrainingSystem.Core.Services.Interface
{
    public interface IAttendanceService
    {
        Task MarkAttendanceAsync(int scheduleId, List<AttendanceRequest> list);
        Task<List<AttendanceResponse>> GetAttendanceByScheduleAsync(int scheduleId);
    }
}
