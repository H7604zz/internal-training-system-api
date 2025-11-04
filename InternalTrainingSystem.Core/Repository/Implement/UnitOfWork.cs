using InternalTrainingSystem.Core.DB;
using InternalTrainingSystem.Core.Repository.Interface;

namespace InternalTrainingSystem.Core.Repository.Implement
{
    public class UnitOfWork : IUnitOfWork
    {
        private readonly ApplicationDbContext _db;
        public UnitOfWork(ApplicationDbContext db) { _db = db; }
        public Task<int> SaveChangesAsync(CancellationToken ct = default) => _db.SaveChangesAsync(ct);
    }
}
