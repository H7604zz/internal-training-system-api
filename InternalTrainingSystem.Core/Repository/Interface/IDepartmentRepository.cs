using InternalTrainingSystem.Core.DTOs;

namespace InternalTrainingSystem.Core.Repository.Interface
{
    public interface IDepartmentRepository
    {
        Task<List<DepartmentDto>> GetDepartments();
    }
}
