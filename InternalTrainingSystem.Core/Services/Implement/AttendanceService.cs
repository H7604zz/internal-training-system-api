using InternalTrainingSystem.Core.DTOs;
using InternalTrainingSystem.Core.Repository.Interface;
using InternalTrainingSystem.Core.Services.Interface;

namespace InternalTrainingSystem.Core.Services.Implement
{
    public class AttendanceService : IAttendanceService
    {
        private readonly IAttendanceRepository _attendanceRepo;

        public AttendanceService(IAttendanceRepository attendanceRepo)
        {
            _attendanceRepo = attendanceRepo;
        }

        public Task<List<AttendanceDto.AttendanceResponse>> GetAttendanceByScheduleAsync(int scheduleId)
        {
            return _attendanceRepo.GetAttendanceByScheduleAsync(scheduleId);
        }

        public async Task MarkAttendanceAsync(int scheduleId, List<AttendanceDto.AttendanceRequest> list)
        {
            await _attendanceRepo.MarkAttendanceAsync(scheduleId, list);
        }

        public async Task<bool> UpdateAttendanceAsync(int scheduleId, List<AttendanceDto.AttendanceRequest> list)
        {
            return await _attendanceRepo.UpdateAttendanceAsync(scheduleId, list);
        }
    }
}
