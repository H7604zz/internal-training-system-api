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

		[HttpGet("{departmentId}")]
		public async Task<IActionResult> GetDepartmentDetail(int departmentId)
		{
			var department = await _departmentService.GetDepartmentDetailAsync(departmentId);
			return Ok(department);
		}

		[HttpPost]
		public async Task<IActionResult> CreateDepartment([FromBody] DepartmentRequestDto request)
		{
			if (!ModelState.IsValid)
				return BadRequest(ModelState);

            var success = await _departmentService.CreateDepartmentAsync(request);
            if (!success)
                return BadRequest();

            return Ok();
		}

		[HttpPut("{departmentId}")]
		public async Task<IActionResult> Update(int departmentId, [FromBody] DepartmentRequestDto request)
		{
			if (!ModelState.IsValid)
				return BadRequest(ModelState);

			var success = await _departmentService.UpdateDepartmentAsync(departmentId, request);
			if (!success)
				return NotFound();

			return Ok();
		}

		[HttpDelete("{departmentId}")]
		public async Task<IActionResult> Delete(int departmentId)
		{
			var success = await _departmentService.DeleteDepartmentAsync(departmentId);
			if (!success)
				return NotFound();

			return Ok();
		}
	}
}