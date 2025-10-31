using InternalTrainingSystem.Core.Configuration;
using InternalTrainingSystem.Core.DTOs;

namespace InternalTrainingSystem.Core.Repository.Interface
{
	public interface IDepartmentRepository
	{
		Task<List<DepartmentDto>> GetDepartments();
		Task<PagedResult<DepartmenDetailsDto>> GetAllDepartmentsAsync(DepartmentInputDto input);
		Task<bool> CreateDepartmentAsync(CreateDepartmentDto department);
		Task<bool> UpdateDepartmentAsync(int id, UpdateDepartmentDto department);
		Task<bool> DeleteDepartmentAsync(int departmentId);
		Task<DepartmenDetailsDto> GetDepartmentByIdAsync(int id);
		Task<DepartmenCourseAndEmployeeDto> GetDepartmentCourseAndEmployeeAsync(DepartmentCourseAndEmployeeInput input);
	}
}
