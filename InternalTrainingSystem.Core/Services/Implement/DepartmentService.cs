using DocumentFormat.OpenXml.Wordprocessing;
using InternalTrainingSystem.Core.Configuration;
using InternalTrainingSystem.Core.DTOs;
using InternalTrainingSystem.Core.Models;
using InternalTrainingSystem.Core.Repository.Interface;
using InternalTrainingSystem.Core.Services.Interface;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace InternalTrainingSystem.Core.Services.Implement
{
	public class DepartmentService : IDepartmentService
	{
		private readonly IDepartmentRepository _departmentRepo;
		public DepartmentService(IDepartmentRepository departmentRepo)
		{
			_departmentRepo = departmentRepo;
		}

		public async Task<List<DepartmentListDto>> GetDepartmentsAsync()
		{
			return await _departmentRepo.GetDepartmentsAsync();
		}

		public async Task<DepartmentDetailDto?> GetDepartmentDetailAsync(DepartmentDetailRequestDto request)
		{
			return await _departmentRepo.GetDepartmentDetailAsync(request);
		}

		public async Task<bool> CreateDepartmentAsync(DepartmentRequestDto department)
		{
			return await _departmentRepo.CreateDepartmentAsync(department);
		}

		public async Task<bool> DeleteDepartmentAsync(int departmentId)
		{
			return await _departmentRepo.DeleteDepartmentAsync(departmentId);
		}

		public async Task<bool> UpdateDepartmentAsync(int id, DepartmentRequestDto department)
		{
			return await _departmentRepo.UpdateDepartmentAsync(id, department);
		}

		public async Task<bool> TransferEmployeeAsync(TransferEmployeeDto request)
		{
			return await _departmentRepo.TransferEmployeeAsync(request);
		}

		public async Task<List<DepartmentCourseCompletionDto>> GetDepartmentCourseCompletionAsync(DepartmentReportRequestDto request)
		{
			return await _departmentRepo.GetDepartmentCourseCompletionAsync(request);
		}

		public async Task<List<TopActiveDepartmentDto>> GetTopActiveDepartmentsAsync(int topCount, DepartmentReportRequestDto request)
		{
			return await _departmentRepo.GetTopActiveDepartmentsAsync(topCount, request);
		}
	}
}