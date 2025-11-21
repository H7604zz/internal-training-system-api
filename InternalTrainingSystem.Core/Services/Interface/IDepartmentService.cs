using InternalTrainingSystem.Core.Configuration;
using InternalTrainingSystem.Core.DTOs;

namespace InternalTrainingSystem.Core.Services.Interface
{
    public interface IDepartmentService
    {
        Task<List<DepartmentListDto>> GetDepartmentsAsync();
        Task<DepartmentDetailDto?> GetDepartmentDetailAsync(DepartmentDetailRequestDto request);
        Task<bool> UpdateDepartmentAsync(int id, DepartmentRequestDto department);
        Task<bool> CreateDepartmentAsync(DepartmentRequestDto department);
        Task<bool> DeleteDepartmentAsync(int departmentId);
        Task<bool> TransferEmployeeAsync(TransferEmployeeDto request);
        Task<List<DepartmentCourseCompletionDto>> GetDepartmentCourseCompletionAsync(DepartmentReportRequestDto request);
        Task<List<TopActiveDepartmentDto>> GetTopActiveDepartmentsAsync(int topCount, DepartmentReportRequestDto request);
    }
}