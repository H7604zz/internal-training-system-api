using InternalTrainingSystem.Core.DTOs;
using InternalTrainingSystem.Core.Models;

namespace InternalTrainingSystem.Core.Services.Interface
{
    public interface IDepartmentService
    {
         Task<List<DepartmentDto>> GetDepartments();
    }
}
