using InternalTrainingSystem.Core.Services.Implement;
using InternalTrainingSystem.Core.Services.Interface;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace InternalTrainingSystem.Core.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UserController : ControllerBase
    {
        private readonly IUserService _userService;

        public UserController(IUserService userServices)
        {
            _userService = userServices;
        }

        [HttpGet("{courseId}/eligible-users")]
        public IActionResult GetEligibleUsers(int courseId)
        { 
            var staffWithoutCertificate = _userService.GetUserRoleStaffWithoutCertificate(courseId);
            return Ok(staffWithoutCertificate);
        }
    }
}
