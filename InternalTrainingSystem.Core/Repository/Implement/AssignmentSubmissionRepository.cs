using InternalTrainingSystem.Core.DB;
using InternalTrainingSystem.Core.Models;
using InternalTrainingSystem.Core.Repository.Interface;
using Microsoft.EntityFrameworkCore;
using System;

namespace InternalTrainingSystem.Core.Repository.Implement
{
    public class AssignmentSubmissionRepository : IAssignmentSubmissionRepository
    {
        private readonly ApplicationDbContext _db;

        public AssignmentSubmissionRepository(ApplicationDbContext db)
        {
            _db = db;
        }

        public Task<AssignmentSubmission?> GetByIdWithFilesAndUserAsync(int submissionId, CancellationToken ct = default)
            => _db.AssignmentSubmissions
                  //.Include(s => s.Files)
                  .Include(s => s.User)
                  .Include(s => s.Assignment)
                  .FirstOrDefaultAsync(s => s.SubmissionId == submissionId, ct);

        public Task<List<AssignmentSubmission>> GetByAssignmentAsync(int assignmentId, CancellationToken ct = default)
            => _db.AssignmentSubmissions
                  .Include(s => s.User)
                  .Where(s => s.AssignmentId == assignmentId)
                  .OrderByDescending(s => s.SubmittedAt)
                  .ToListAsync(ct);

        public Task<List<AssignmentSubmission>> GetByAssignmentAndUserAsync(
            int assignmentId, string userId, CancellationToken ct = default)
            => _db.AssignmentSubmissions
                  //.Include(s => s.Files)
                  .Where(s => s.AssignmentId == assignmentId && s.UserId == userId)
                  .OrderByDescending(s => s.SubmittedAt)
                  .ToListAsync(ct);

        public async Task<int> GetMaxAttemptNumberAsync(int assignmentId, string userId, CancellationToken ct = default)
        {
            var attempt = await _db.AssignmentSubmissions
                .Where(s => s.AssignmentId == assignmentId && s.UserId == userId)
                .MaxAsync(s => (int?)s.AttemptNumber, ct);

            return attempt ?? 0;
        }

        public async Task AddAsync(AssignmentSubmission entity, CancellationToken ct = default)
            => await _db.AssignmentSubmissions.AddAsync(entity, ct);

        public void Update(AssignmentSubmission entity)
            => _db.AssignmentSubmissions.Update(entity);
    }
}
