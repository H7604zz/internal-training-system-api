using InternalTrainingSystem.Core.DB;
using InternalTrainingSystem.Core.Models;
using InternalTrainingSystem.Core.Repository.Interface;
using Microsoft.EntityFrameworkCore;
using System;

namespace InternalTrainingSystem.Core.Repository.Implement
{
    public class SubmissionFileRepository : ISubmissionFileRepository
    {
        private readonly ApplicationDbContext _db;

        public SubmissionFileRepository(ApplicationDbContext db)
        {
            _db = db;
        }

        public async Task AddRangeAsync(IEnumerable<SubmissionFile> files, CancellationToken ct = default)
            => await _db.SubmissionFiles.AddRangeAsync(files, ct);

        public async Task RemoveBySubmissionIdAsync(int submissionId, CancellationToken ct = default)
        {
            var files = await _db.SubmissionFiles
                .Where(f => f.SubmissionId == submissionId)
                .ToListAsync(ct);
            _db.SubmissionFiles.RemoveRange(files);
        }
    }
}
