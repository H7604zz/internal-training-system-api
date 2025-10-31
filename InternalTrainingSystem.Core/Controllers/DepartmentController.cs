using InternalTrainingSystem.Core.DTOs;
using InternalTrainingSystem.Core.Services.Interface;
using Microsoft.AspNetCore.Mvc;

namespace InternalTrainingSystem.Core.Controllers
{
	[ApiController]
	[Route("api/[controller]")]
	public class DepartmentController : ControllerBase
	{
		private readonly IDepartmentService _departmentService;

		public DepartmentController(IDepartmentService departmentService)
		{
			_departmentService = departmentService;
		}

		[HttpGet]
		public async Task<IActionResult> GetDepartments()
		{
			var departments = await _departmentService.GetDepartmentsAsync();

			return Ok(departments);
		}

		[HttpGet("/{id}")]
		public async Task<IActionResult> GetDepartmentDetail(int id)
		{
			var department = await _departmentService.GetDepartmentByIdAsync(id);
			return Ok(department);
		}

		[HttpPost]
		public async Task<IActionResult> CreateDepartment([FromBody] CreateDepartmentDto input)
		{
			if (!ModelState.IsValid)
				return BadRequest(ModelState);

			var id = await _departmentService.CreateDepartmentAsync(input);
			return Ok(new { Id = id});
		}

		[HttpPut("{id:int}")]
		public async Task<IActionResult> Update(int id, [FromBody] UpdateDepartmentDto dto)
		{
			if (!ModelState.IsValid)
				return BadRequest(ModelState);
			var success = await _departmentService.UpdateDepartmentAsync(id, dto);
			if (!success)
				return NotFound();
			return NoContent();
		}

		[HttpDelete("{id:int}")]
		public async Task<IActionResult> Delete(int id)
		{
			var success = await _departmentService.DeleteDepartmentAsync(id);
			if (!success)
				return NotFound();
			return NoContent();
		}
	}
}