using InternalTrainingSystem.Core.Models;

namespace InternalTrainingSystem.Core.Repository.Interface
{
    public interface IAssignmentRepository
    {
        Task<Assignment?> GetByIdAsync(int id, CancellationToken ct = default);
        Task<Assignment?> GetWithClassAsync(int id, CancellationToken ct = default);
        Task<List<Assignment>> GetByClassAsync(int classId, CancellationToken ct = default);
        Task AddAsync(Assignment entity, CancellationToken ct = default);
        void Update(Assignment entity);
        void Remove(Assignment entity);
    }
}
