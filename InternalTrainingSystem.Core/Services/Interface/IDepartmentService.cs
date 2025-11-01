using InternalTrainingSystem.Core.Configuration;
using InternalTrainingSystem.Core.DTOs;

namespace InternalTrainingSystem.Core.Services.Interface
{
    public interface IDepartmentService
    {
        Task<List<DepartmentListDto>> GetDepartmentsAsync();
        Task<DepartmentDetailDto?> GetDepartmentDetailAsync(int departmentId);
        Task<bool> UpdateDepartmentAsync(int id, DepartmentRequestDto department);
        Task<bool> CreateDepartmentAsync(DepartmentRequestDto department);
        Task<bool> DeleteDepartmentAsync(int departmentId);
    }
}