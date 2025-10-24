using InternalTrainingSystem.Core.Configuration;
using InternalTrainingSystem.Core.Constants;
using InternalTrainingSystem.Core.DB;
using InternalTrainingSystem.Core.DTOs;
using InternalTrainingSystem.Core.Models;
using InternalTrainingSystem.Core.Services.Interface;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;

namespace InternalTrainingSystem.Core.Services.Implement
{
    public class UserService : IUserService
    {
        private readonly ApplicationDbContext _context;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IEmailSender _emailSender;
        private readonly IConfiguration _config;

        public UserService(UserManager<ApplicationUser> userManager, RoleManager<IdentityRole> roleManager, ApplicationDbContext context, IEmailSender emailSender, IConfiguration config)
        {
            _context = context;
            _userManager = userManager;
            _roleManager = roleManager;
            _emailSender = emailSender;
            _config = config;
        }

        public PagedResult<StaffConfirmCourseResponse> GetUserRoleStaffConfirmCourse(int courseId, int page, int pageSize)
        {
            var query = _context.Users
                .Include(u => u.CourseEnrollments)
                .Include(u => u.Department)
                .Join(_context.UserRoles, u => u.Id, ur => ur.UserId, (u, ur) => new { u, ur })
                .Join(_context.Roles, x => x.ur.RoleId, r => r.Id, (x, r) => new { x.u, r })
                .Where(x => x.r.Name == UserRoles.Staff
                            && _context.CourseEnrollments.Any(c => c.UserId == x.u.Id
                                                                   && c.CourseId == courseId
                                                                   && c.Status == EnrollmentConstants.Status.Enrolled))
                .Select(x => x.u)
                .Distinct()
                .AsQueryable();

            int totalCount = query.Count();

            var items = query
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(u => new StaffConfirmCourseResponse
                {
                    EmployeeId = u.EmployeeId,
                    FullName = u.FullName,
                    Email = u.Email!,
                    Department = u.Department!.Name,
                    Position = u.Position,
                    Status = EnrollmentConstants.Status.Enrolled,
                })
                .ToList();

            return new PagedResult<StaffConfirmCourseResponse>
            {
                Items = items,
                TotalCount = totalCount,
                Page = page,
                PageSize = pageSize
            };
        }

        public PagedResult<EligibleStaffResponse> GetUserRoleEligibleStaff(int courseId, int page, int pageSize)
        {
            var query = from u in _context.Users
                        join ur in _context.UserRoles on u.Id equals ur.UserId
                        join r in _context.Roles on ur.RoleId equals r.Id
                        join d in _context.Departments on u.DepartmentId equals d.Id
                        join e in _context.CourseEnrollments
                            .Where(x => x.CourseId == courseId)
                            on u.Id equals e.UserId into ue
                        from e in ue.DefaultIfEmpty()
                        where r.Name == UserRoles.Staff
                              && _context.Courses
                                  .Where(c => c.CourseId == courseId)
                                  .SelectMany(c => c.Departments)
                                  .Any(dep => dep.Id == d.Id)
                              && !_context.Certificates
                                  .Any(c => c.UserId == u.Id && c.CourseId == courseId)
                        orderby u.FullName
                        select new EligibleStaffResponse
                        {
                            EmployeeId = u.EmployeeId,
                            FullName = u.FullName,
                            Email = u.Email!,
                            Department = d.Name,
                            Position = u.Position,
                            Status = e.Status ?? EnrollmentConstants.Status.NotEnrolled,
                            Reason = e.RejectionReason ?? "Chưa có phản hồi"
                        };

            int totalCount = query.Count();

            var items = query
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToList();

            return new PagedResult<EligibleStaffResponse>
            {
                Items = items,
                TotalCount = totalCount,
                Page = page,
                PageSize = pageSize
            };
        }

        public List<ApplicationUser> GetUsersByRole(string role)
        {
            var mentors = _context.Users
                .Join(_context.UserRoles, u => u.Id, ur => ur.UserId, (u, ur) => new { u, ur })
                .Join(_context.Roles, x => x.ur.RoleId, r => r.Id, (x, r) => new { x.u, r })
                .Where(x => x.r.Name == role && x.u.IsActive)
                .Select(x => x.u)
                .Distinct()
                .ToList();

            return mentors;
        }

        //Tạo mật khẩu ngẫu nhiên
        public static string Generate(PasswordOptions opts)
        {
            int length = Math.Max(12, opts.RequiredLength);

            string upper = "ABCDEFGHJKLMNPQRSTUVWXYZ";
            string lower = "abcdefghijkmnopqrstuvwxyz";
            string digits = "0123456789";
            string nonAlnum = "!@$?_-";
            string all = upper + lower + digits + nonAlnum;

            string Take(string src)
            {
                byte[] b = new byte[1];
                RandomNumberGenerator.Fill(b);
                return src[b[0] % src.Length].ToString();
            }

            var sb = new StringBuilder();

            if (opts.RequireUppercase) sb.Append(Take(upper));
            if (opts.RequireLowercase) sb.Append(Take(lower));
            if (opts.RequireDigit) sb.Append(Take(digits));
            if (opts.RequireNonAlphanumeric) sb.Append(Take(nonAlnum));

            while (sb.Length < length) sb.Append(Take(all));
            return sb.ToString();
        }

        public async Task<IActionResult> CreateUserAsync(CreateUserDto req)
        {
            // Validate
            if (!await _context.Departments.AnyAsync(d => d.Id == req.DepartmentId))
                return new BadRequestObjectResult("Department không tồn tại.");

            if (await _context.Users.SingleOrDefaultAsync(m=>m.Email==req.Email) != null)
                return new ConflictObjectResult("Email đã tồn tại.");

            var roleName = string.IsNullOrWhiteSpace(req.RoleName) ? "Staff" : req.RoleName.Trim();
            if (!UserRoles.All.Contains(roleName, StringComparer.OrdinalIgnoreCase))
                return new BadRequestObjectResult($"Role '{roleName}' không hợp lệ.");

            // Map sang ApplicationUser (UserName = Email)
            var user = new ApplicationUser
            {
                UserName = req.Email,
                Email = req.Email,
                PhoneNumber = req.Phone,
                FullName = req.FullName,
                Position = req.Position,
                DepartmentId = req.DepartmentId,
                IsActive = true,
                CreatedDate = DateTime.UtcNow
            };

            // Tạo user với mật khẩu tạm
            var tempPassword = Generate(_userManager.Options.Password);
            var createResult = await _userManager.CreateAsync(user, tempPassword);
            if (!createResult.Succeeded)
                return new BadRequestObjectResult(string.Join("; ",
                    createResult.Errors.Select(e => $"{e.Code}: {e.Description}")));

            // Bảo đảm chỉ 1 role
            if (!await _roleManager.RoleExistsAsync(roleName))
                await _roleManager.CreateAsync(new IdentityRole(roleName));

            var currentRoles = await _userManager.GetRolesAsync(user);
            if (currentRoles.Any())
                await _userManager.RemoveFromRolesAsync(user, currentRoles);

            var addRoleResult = await _userManager.AddToRoleAsync(user, roleName);
            if (!addRoleResult.Succeeded)
                return new BadRequestObjectResult(string.Join("; ",
                    addRoleResult.Errors.Select(e => e.Description)));

            // Claim bắt đổi mật khẩu lần đầu (khuyến nghị)
            await _userManager.AddClaimAsync(user, new Claim("MustChangePassword", "true"));

            // Link xác nhận email
            var token = await _userManager.GenerateEmailConfirmationTokenAsync(user);
            var baseUrl = _config["App:FrontendBaseUrl"]?.TrimEnd('/') ?? "";
            var confirmUrl =
                $"{baseUrl}/account/confirm-email?userId={Uri.EscapeDataString(user.Id)}&token={Uri.EscapeDataString(token)}";

            // Gửi email
            var subject = "Kích hoạt tài khoản hệ thống đào tạo";
            var body = $@"
Chào {user.FullName},

Tài khoản của bạn đã được tạo trên hệ thống đào tạo nội bộ.

Tên đăng nhập (Email): {user.Email}
Mật khẩu tạm:          {tempPassword}

Vui lòng xác nhận email và đăng nhập tại:
{confirmUrl}

Hãy đổi mật khẩu ngay sau khi đăng nhập.

Trân trọng,
Phòng IT
";
            await _emailSender.SendEmailAsync(user.Email!, subject, body);

            // Trả về Ok
            var result = new
            {
                user.Id,
                user.Email,
                user.FullName,
                user.Position,
                user.PhoneNumber,
                user.DepartmentId,
                Role = roleName
            };
            return new OkObjectResult(result);
        }
    }
}