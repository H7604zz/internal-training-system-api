namespace InternalTrainingSystem.Core.Repository.Interface
{
    public interface IUnitOfWork
    {
        Task<int> SaveChangesAsync(CancellationToken ct = default);
    }
}
