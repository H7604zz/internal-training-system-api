using InternalTrainingSystem.Core.Configuration;
using InternalTrainingSystem.Core.Constants;
using InternalTrainingSystem.Core.DB;
using InternalTrainingSystem.Core.DTOs;
using InternalTrainingSystem.Core.Helper;
using InternalTrainingSystem.Core.Models;
using InternalTrainingSystem.Core.Services.Interface;
using InternalTrainingSystem.Core.Utils;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.WebUtilities;
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
        private readonly IConfiguration _config;

        public UserService(UserManager<ApplicationUser> userManager, RoleManager<IdentityRole> roleManager, ApplicationDbContext context, IConfiguration config)
        {
            _context = context;
            _userManager = userManager;
            _roleManager = roleManager;
            _config = config;
        }

        public PagedResult<StaffConfirmCourseResponse> GetStaffConfirmCourse(int courseId, int page, int pageSize)
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

        public PagedResult<EligibleStaffResponse> GetEligibleStaff(int courseId, UserSearchDto searchDto)
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
                             && (string.IsNullOrEmpty(searchDto.SearchTerm)
                                  || u.FullName.Contains(searchDto.SearchTerm)
                                  || u.EmployeeId!.Contains(searchDto.SearchTerm)
                                  || u.Email!.Contains(searchDto.SearchTerm))
                              && (string.IsNullOrEmpty(searchDto.status)
                                  || (e != null && e.Status == searchDto.status))
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
                .Skip((searchDto.Page - 1) * searchDto.PageSize)
                .Take(searchDto.PageSize)
                .ToList();

            return new PagedResult<EligibleStaffResponse>
            {
                Items = items,
                TotalCount = totalCount,
                Page = searchDto.Page,
                PageSize = searchDto.PageSize
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


        public async Task<bool> CreateUserAsync(CreateUserDto req)
        {
            

            // Validate role
            var roleName = string.IsNullOrWhiteSpace(req.RoleName) ? "Staff" : req.RoleName.Trim();
            // ✅ Kiểm tra EmployeeId đã tồn tại chưa
            var existingUser = await _userManager.Users
                .FirstOrDefaultAsync(u => u.EmployeeId == req.EmployeeId);

            if (existingUser != null)
            {
                // Có thể throw exception hoặc return false tùy yêu cầu
                throw new InvalidOperationException($"EmployeeId '{req.EmployeeId}' đã tồn tại trong hệ thống.");
            }

            // Map sang ApplicationUser
            var user = new ApplicationUser
            {
                EmployeeId=req.EmployeeId,
                UserName = req.Email,
                Email = req.Email,
                PhoneNumber = req.Phone,
                FullName = req.FullName,
                Position = req.Position,
                DepartmentId = req.DepartmentId,
                IsActive = false,
                CreatedDate = DateTime.Now
            };

            // Tạo user với mật khẩu tạm
            var tempPassword = PasswordUtils.Generate(_userManager.Options.Password);
            var createResult = await _userManager.CreateAsync(user, tempPassword);
            if (!createResult.Succeeded)
                return false;

            // Đảm bảo chỉ 1 role
            if (!await _roleManager.RoleExistsAsync(roleName))
                await _roleManager.CreateAsync(new IdentityRole(roleName));

            var currentRoles = await _userManager.GetRolesAsync(user);
            if (currentRoles.Any())
                await _userManager.RemoveFromRolesAsync(user, currentRoles);

            var addRoleResult = await _userManager.AddToRoleAsync(user, roleName);
            if (!addRoleResult.Succeeded)
                return false;

            // Bắt đổi mật khẩu lần đầu
            await _userManager.AddClaimAsync(user, new Claim("MustChangePassword", "true"));

            // Link xác nhận email
            var token = await _userManager.GenerateEmailConfirmationTokenAsync(user);
            var baseUrl = _config["App:FrontendBaseUrl"]?.TrimEnd('/') ?? "https://localhost:7001";
            var confirmUrl =
                $"{baseUrl}/account/confirm-email?userId={Uri.EscapeDataString(user.Id)}&token={Uri.EscapeDataString(token)}";

            // Gửi email HTML
            var subject = "Kích hoạt tài khoản hệ thống đào tạo";
            var body = $@"
                <html>
                <body style='font-family:Arial, sans-serif; line-height:1.6; color:#333'>
                    <p>Chào <strong>{user.FullName}</strong>,</p>

                    <p>Tài khoản của bạn đã được tạo trên <strong>hệ thống đào tạo nội bộ</strong>.</p>

                    <p>
                        <b>Tên đăng nhập (Email):</b> {user.Email}<br/>
                        <b>Mật khẩu tạm:</b> {tempPassword}
                    </p>

                    <p>
                        Vui lòng xác nhận email và đăng nhập tại liên kết bên dưới:<br/>
                        <a href='{confirmUrl}' style='color:#1a73e8;text-decoration:none;' target='_blank'>
                            Xác nhận tài khoản của tôi
                        </a>
                    </p>

                    <p><i>Hãy đổi mật khẩu ngay sau khi đăng nhập.</i></p>

                    <p>Trân trọng,<br/>
                    <b>Phòng IT</b></p>
                </body>
                </html>";
            await EmailHelper.SendEmailAsync(user.Email!, subject, body);

            return true;
        }

        public async Task<bool> VerifyAccountAsync(string userId, string token)
        {
            if (string.IsNullOrWhiteSpace(userId) || string.IsNullOrWhiteSpace(token))
                return false;

            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
                return false;

            if (user.EmailConfirmed)
                return true;

            string usableToken;
            try
            {
                var decodedBytes = WebEncoders.Base64UrlDecode(token);
                usableToken = Encoding.UTF8.GetString(decodedBytes);
            }
            catch
            {
                usableToken = Uri.UnescapeDataString(token)
                                 .Replace(" ", "+");
            }

            var result = await _userManager.ConfirmEmailAsync(user, usableToken);
            if (!result.Succeeded)
            {
                var errors = string.Join("; ", result.Errors.Select(e => e.Description));
                Console.WriteLine($"❌ Xác nhận email thất bại: {errors}");
                return false;
            }

            user.IsActive = true;
            await _userManager.UpdateAsync(user);

            Console.WriteLine($"✅ Email xác nhận thành công cho user {user.Email}");
            return true;
        }

        public List<IdentityRole> GetRoles()
        {
           return _context.Roles.ToList();
        }

        public async Task<ApplicationUser?> GetUserProfileAsync(string userId)
        {
            return await _context.Users
                .Include(u => u.Department)
                .FirstOrDefaultAsync(u => u.Id == userId);
        }


        
    }
}