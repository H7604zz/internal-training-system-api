using static InternalTrainingSystem.Core.DTOs.AttendanceDto;

namespace InternalTrainingSystem.Core.Services.Interface
{
    public interface IAttendanceService
    {
        Task MarkAttendanceAsync(int scheduleId, List<AttendanceRequest> list);
        Task<bool> UpdateAttendanceAsync(int scheduleId, List<AttendanceRequest> list);
        Task<List<AttendanceResponse>> GetAttendanceByScheduleAsync(int scheduleId);
    }
}
