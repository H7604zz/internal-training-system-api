using InternalTrainingSystem.Core.DB;
using InternalTrainingSystem.Core.Models;
using InternalTrainingSystem.Core.Repository.Interface;
using Microsoft.EntityFrameworkCore;
using System;

namespace InternalTrainingSystem.Core.Repository.Implement
{
    public class AssignmentRepository : IAssignmentRepository
    {
        private readonly ApplicationDbContext _db;

        public AssignmentRepository(ApplicationDbContext db)
        {
            _db = db;
        }

        public Task<Assignment?> GetByIdAsync(int id, CancellationToken ct = default)
            => _db.Assignments.FirstOrDefaultAsync(a => a.AssignmentId == id, ct);

        public Task<Assignment?> GetWithClassAsync(int id, CancellationToken ct = default)
            => _db.Assignments
                  .Include(a => a.Class)
                  .FirstOrDefaultAsync(a => a.AssignmentId == id, ct);

        public Task<List<Assignment>> GetByClassAsync(int classId, CancellationToken ct = default)
            => _db.Assignments
                  .Where(a => a.ClassId == classId)
                  .OrderBy(a => a.DueAt)
                  .ToListAsync(ct);

        public async Task AddAsync(Assignment entity, CancellationToken ct = default)
        {
            await _db.Assignments.AddAsync(entity, ct);
            await _db.SaveChangesAsync();
        }
        public void Update(Assignment entity)
            => _db.Assignments.Update(entity);

        public void Remove(Assignment entity)
            => _db.Assignments.Remove(entity);
        public Task<bool> ExistsInClassAsync(int classId, CancellationToken ct)
        => _db.Assignments.AnyAsync(a => a.ClassId == classId, ct);
    }
}
