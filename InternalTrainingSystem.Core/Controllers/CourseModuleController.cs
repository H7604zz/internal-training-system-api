using InternalTrainingSystem.Core.DTOs;
using InternalTrainingSystem.Core.Services.Interface;
using Microsoft.AspNetCore.Mvc;

namespace InternalTrainingSystem.Core.Controllers
{
        [ApiController]
        [Route("api/courses/{courseId:int}/modules")]
        public class CourseModuleController : ControllerBase
        {
            private readonly ICourseMaterialService _courseMaterialService;

            public CourseModuleController(ICourseMaterialService cms)
            {
                _courseMaterialService = cms;
            }

            // GET: /api/courses/{courseId}/modules
            [HttpGet]
            public async Task<ActionResult<IEnumerable<ModuleDetailDto>>> GetModules(int courseId, CancellationToken ct)
            {
                var data = await _courseMaterialService.GetModulesByCourseAsync(courseId, ct);
                return Ok(data);
            }

            // GET: /api/courses/{courseId}/modules/{moduleId}
            [HttpGet("{moduleId:int}")]
            public async Task<ActionResult<ModuleDetailDto>> GetModule(int courseId, int moduleId, CancellationToken ct)
            {
                var mod = await _courseMaterialService.GetModuleAsync(moduleId, ct);
                if (mod == null || mod.CourseId != courseId)
                    return NotFound(new { message = "Module not found" });
                return Ok(mod);
            }

            // POST: /api/courses/{courseId}/modules
            [HttpPost]
            public async Task<ActionResult> CreateModule(int courseId, [FromBody] CreateModuleDto dto, CancellationToken ct)
            {
                if (courseId != dto.CourseId)
                    return BadRequest(new { message = "courseId in route and body must match" });

                var created = await _courseMaterialService.CreateModuleAsync(dto, ct);
                return CreatedAtAction(nameof(GetModule),
                    new { courseId, moduleId = created.Id },
                    new { created.Id, created.CourseId, created.Title });
            }

            // PUT: /api/courses/{courseId}/modules/{moduleId}
            [HttpPut("{moduleId:int}")]
            public async Task<IActionResult> UpdateModule(int courseId, int moduleId, [FromBody] UpdateModuleDto dto, CancellationToken ct)
            {
                var mod = await _courseMaterialService.GetModuleAsync(moduleId, ct);
                if (mod == null || mod.CourseId != courseId)
                    return NotFound(new { message = "Module not found" });

                var ok = await _courseMaterialService.UpdateModuleAsync(moduleId, dto, ct);
                return ok ? NoContent() : NotFound(new { message = "Update failed" });
            }

            // DELETE: /api/courses/{courseId}/modules/{moduleId}
            [HttpDelete("{moduleId:int}")]
            public async Task<IActionResult> DeleteModule(int courseId, int moduleId, CancellationToken ct)
            {
                var mod = await _courseMaterialService.GetModuleAsync(moduleId, ct);
                if (mod == null || mod.CourseId != courseId)
                    return NotFound(new { message = "Module not found" });

                var ok = await _courseMaterialService.DeleteModuleAsync(moduleId, ct);
                return ok ? NoContent() : NotFound(new { message = "Delete failed" });
            }
        }
    }
