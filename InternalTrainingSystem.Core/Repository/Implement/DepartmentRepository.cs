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

		public async Task<Models.Department> GetDepartmentByIdAsync(int id)
		{
			var department = await _context.Departments.FindAsync(id);
			if (department == null)
			{
				throw new KeyNotFoundException("Department not found");
			}
			return department;
		}

		public async Task<Department> GetDepartmentCourseAndEmployeeAsync(int departmentId)
		{
			var department = await _context.Departments
							.Include(d => d.Users) 
							.Include(d => d.Courses)   
							.FirstOrDefaultAsync(d => d.Id == departmentId);
			if(department == null)
			{
				throw new KeyNotFoundException("department not found");
			}
			return department;
		}

		public async Task<List<DepartmentDto>> GetDepartmentsAsync()
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
