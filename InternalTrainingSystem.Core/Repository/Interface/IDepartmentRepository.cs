using InternalTrainingSystem.Core.Configuration;
using InternalTrainingSystem.Core.DTOs;
using InternalTrainingSystem.Core.Models;

namespace InternalTrainingSystem.Core.Repository.Interface
{
    public interface IDepartmentRepository
    {
        Task<List<DepartmentListDto>> GetDepartmentsAsync();
        Task<bool> CreateDepartmentAsync(DepartmentRequestDto department);
        Task<bool> UpdateDepartmentAsync(int id, DepartmentRequestDto department);
        Task<bool> DeleteDepartmentAsync(int departmentId);
        Task<DepartmentDetailDto> GetDepartmentDetailAsync(int id);
    }
}