using InternalTrainingSystem.Core.Models;

namespace InternalTrainingSystem.Core.Repository.Interface
{
    public interface IAssignmentRepository
    {
        Task<Assignment?> GetByIdAsync(int id, CancellationToken ct = default);
        Task<Assignment?> GetWithClassAsync(int id, CancellationToken ct = default);
        Task<Assignment?> GetSingleByClassAsync(int classId, CancellationToken ct);
        Task AddAsync(Assignment entity, CancellationToken ct = default);
        void Update(Assignment entity);
        void Remove(Assignment entity);
        Task<bool> ExistsInClassAsync(int classId, CancellationToken ct);
    }
}
