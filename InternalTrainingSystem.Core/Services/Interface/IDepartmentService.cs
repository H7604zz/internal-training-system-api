using InternalTrainingSystem.Core.Configuration;
using InternalTrainingSystem.Core.DTOs;

namespace InternalTrainingSystem.Core.Services.Interface
{
	public interface IDepartmentService
	{
		Task<List<DepartmentDto>> GetDepartments();
		Task<PagedResult<DepartmenDetailsDto>> GetAllDepartmentsAsync(DepartmentInputDto input);
		Task<DepartmenDetailsDto?> GetDepartmentByIdAsync(int departmentId);
		Task<DepartmenCourseAndEmployeeDto?> GetDepartmentCourseAndEmployeeAsync(DepartmentCourseAndEmployeeInput input);
		Task<bool> UpdateDepartmentAsync(int id, UpdateDepartmentDto input);
		Task<bool> CreateDepartmentAsync(CreateDepartmentDto input);
		Task<bool> DeleteDepartmentAsync(int departmentId);
	}
}
