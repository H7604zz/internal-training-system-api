using Amazon.Runtime.Internal.Util;
using DocumentFormat.OpenXml.Office2010.Excel;
using InternalTrainingSystem.Core.Configuration;
using InternalTrainingSystem.Core.DB;
using InternalTrainingSystem.Core.DTOs;
using InternalTrainingSystem.Core.Models;
using InternalTrainingSystem.Core.Repository.Interface;
using Microsoft.EntityFrameworkCore;

namespace InternalTrainingSystem.Core.Repository.Implement
{
	public class DepartmentRepository : IDepartmentRepository
	{
		private readonly ApplicationDbContext _context;
		public DepartmentRepository(ApplicationDbContext context)
		{
			_context = context;
		}

		public async Task<Models.Department> AddDepartmentAsync(Models.Department department)
		{
			if (department == null)
			{
				throw new ArgumentNullException(nameof(department));
			}
			await _context.Departments.AddAsync(department);
			await _context.SaveChangesAsync();
			return department;
		}

		public async Task DeleteDepartmentAsync(int departmentId)
		{
			var department = await _context.Departments.FindAsync(departmentId);
			if (department == null)
			{
				throw new KeyNotFoundException("Department not found");
			}
			_context.Departments.Remove(department);
			await _context.SaveChangesAsync();
		}

		public async Task<PagedResult<Department>> GetAllDepartmentsAsync(int page, int pageSize)
		{
			if (page <= 0) page = 1;
			if (pageSize <= 0) pageSize = 10;
			var query = _context.Departments;
			var totalItems = await query.CountAsync();
			var departments = await query.Skip((page - 1)*pageSize)
				.Take(pageSize)
				.ToListAsync();
			return new PagedResult<Department>
			{
				Items = departments,
				TotalCount = totalItems,
				Page = page,
				PageSize = pageSize
			};
		}

		public async Task<Models.Department> GetDepartmentByIdAsync(int id)
		{
			var department = await _context.Departments.FindAsync(id);
			if (department == null)
			{
				throw new KeyNotFoundException("Department not found");
			}
			return department;
		}

		public async Task<Department> GetDepartmentCourseAndEmployeeAsync(int departmentId, string? search, int page, int pageSize)
		{
			var department = await _context.Departments
			 .FirstOrDefaultAsync(d => d.Id == departmentId);

			if (department == null)
				throw new KeyNotFoundException("Department not found");

			department.Courses = await _context.Courses
					.Where(c => c.Departments.Any(d => d.Id == departmentId) &&
											(string.IsNullOrEmpty(search) || c.CourseName.Contains(search)))
					.OrderBy(c => c.CourseId)
					.Skip((page - 1) * pageSize)
					.Take(pageSize)
					.ToListAsync();

			department.Users = await _context.Users
					.Where(u => u.DepartmentId == departmentId &&
											(string.IsNullOrEmpty(search) || u.FullName.Contains(search)))
					.OrderBy(u => u.Id)
					.Skip((page - 1) * pageSize)
					.Take(pageSize)
					.ToListAsync();

			return department;
		}

		public async Task<List<DepartmentDto>> GetDepartments()
		{
			return await _context.Departments
					.Select(d => new DepartmentDto
					{
						DepartmentId = d.Id,
						DepartmentName = d.Name,
					})
					.ToListAsync();
		}

		public async Task UpdateDepartmentAsync(Models.Department department)
		{
			var existingDepartment = await _context.Departments.FindAsync(department.Id);
			if (existingDepartment == null)
			{
				throw new KeyNotFoundException("Department not found");
			}
			existingDepartment.Name = department.Name;
			existingDepartment.Description = department.Description;
			await _context.SaveChangesAsync();
		}
	}
}
