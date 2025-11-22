using InternalTrainingSystem.Core.DB;
using InternalTrainingSystem.Core.DTOs;
using InternalTrainingSystem.Core.Models;
using InternalTrainingSystem.Core.Repository.Interface;
using Microsoft.EntityFrameworkCore;

namespace InternalTrainingSystem.Core.Repository.Implement
{
	public class AttendanceRepository : IAttendanceRepository
	{
		private readonly ApplicationDbContext _context;

		public AttendanceRepository(ApplicationDbContext context)
		{
			_context = context;
		}
        public async Task MarkAttendanceAsync(int scheduleId, List<AttendanceRequest> list)
        {
            foreach (var item in list)
            {
                var participantExists = await _context.ScheduleParticipants
                    .AnyAsync(sp => sp.ScheduleId == scheduleId && sp.UserId == item.UserId);
                if (!participantExists)
                    continue;

                var existing = await _context.Attendances
                    .FirstOrDefaultAsync(a => a.ScheduleId == scheduleId && a.UserId == item.UserId);

                if (existing != null)
                {
                    existing.Status = item.Status!;
                }
            }

            await _context.SaveChangesAsync();
        }

        public async Task<List<AttendanceResponse>> GetAttendanceByScheduleAsync(int scheduleId)
        {
            var attendances = await _context.Attendances
                .Include(a => a.User)
                .Where(a => a.ScheduleId == scheduleId)
                .Select(a => new AttendanceResponse
                {
                    UserId = a.UserId,
                    FullName = a.User.FullName,
                    Email = a.User.Email,
                    Status = a.Status,
                    CheckOutTime = a.CheckOutTime,
                })
                .ToListAsync();

            return attendances;
        }

    }
}
