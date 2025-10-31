using InternalTrainingSystem.Core.Configuration;
using InternalTrainingSystem.Core.DTOs;
using InternalTrainingSystem.Core.Repository.Interface;
using InternalTrainingSystem.Core.Services.Interface;

namespace InternalTrainingSystem.Core.Services.Implement
{
	public class DepartmentService : IDepartmentService
	{
		private readonly IDepartmentRepository _departmentRepo;
		public DepartmentService(IDepartmentRepository departmentRepo)
		{
			_departmentRepo = departmentRepo;
		}

		public async Task<bool> CreateDepartmentAsync(CreateDepartmentDto input)
		{
			return await _departmentRepo.CreateDepartmentAsync(input);
		}

		public async Task<bool> DeleteDepartmentAsync(int departmentId)
		{
			return await _departmentRepo.DeleteDepartmentAsync(departmentId);
		}

		public async Task<PagedResult<DepartmenDetailsDto>> GetAllDepartmentsAsync(DepartmentInputDto input)
		{
			return await _departmentRepo.GetAllDepartmentsAsync(input);
		}

		public async Task<DepartmenDetailsDto?> GetDepartmentByIdAsync(int departmentId)
		{
			return await _departmentRepo.GetDepartmentByIdAsync(departmentId);
		}

		public async Task<DepartmenCourseAndEmployeeDto?> GetDepartmentCourseAndEmployeeAsync(DepartmentCourseAndEmployeeInput input)
		{
			return await _departmentRepo.GetDepartmentCourseAndEmployeeAsync(input);
		}

		public async Task<List<DepartmentDto>> GetDepartments()
		{
			return await _departmentRepo.GetDepartments();
		}

		public async Task<bool> UpdateDepartmentAsync(int id, UpdateDepartmentDto input)
		{
			return await _departmentRepo.UpdateDepartmentAsync(id, input);
		}
	}
}
