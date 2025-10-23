using InternalTrainingSystem.Core.DB;
using InternalTrainingSystem.Core.DTOs;
using InternalTrainingSystem.Core.Models;
using InternalTrainingSystem.Core.Services.Interface;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using InternalTrainingSystem.Core.Configuration;
using InternalTrainingSystem.Core.Constants;

namespace InternalTrainingSystem.Core.Services.Implement
{
    public class CourseService : ICourseService
    {
        private readonly ApplicationDbContext _context;
        private const double AverageRatingPass = 4.5;

        public CourseService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<Course?> CreateCourseAsync(Course course, List<int>? departmentIds)
        {
            if (course == null) throw new ArgumentNullException(nameof(course));

            // Nạp Departments theo danh sách ID (nếu có)
            if (departmentIds is { Count: > 0 })
            {
                var departments = await _context.Departments
                    .Where(d => departmentIds.Contains(d.Id))
                    .ToListAsync();

                // (tùy chọn) Kiểm tra ID không hợp lệ
                var foundIds = departments.Select(d => d.Id).ToHashSet();
                var missing = departmentIds.Where(id => !foundIds.Contains(id)).ToList();
                if (missing.Count > 0)
                {
                    // Có thể throw, return null, hoặc logging rồi bỏ qua
                    throw new ArgumentException($"Department ID không tồn tại: {string.Join(", ", missing)}");
                }

                // Gán navigation collection
                course.Departments = departments;
            }

            await _context.Courses.AddAsync(course);
            await _context.SaveChangesAsync();
            return course;
        }


        public async Task<bool> DeleteCourseAsync(int id)
        {
            // Nạp quan hệ nếu cần (nếu có ràng buộc FK hoặc nhiều bảng con)
            var course = await _context.Courses
                .Include(c => c.Departments)        // many-to-many
                .Include(c => c.CourseEnrollments)  // one-to-many
                .FirstOrDefaultAsync(c => c.CourseId == id);

            if (course == null)
                return false;

            // Nếu chưa cấu hình cascade delete, phải xử lý quan hệ thủ công
            if (course.Departments != null && course.Departments.Count > 0)
                course.Departments.Clear();

            if (course.CourseEnrollments != null && course.CourseEnrollments.Count > 0)
                _context.CourseEnrollments.RemoveRange(course.CourseEnrollments);

            _context.Courses.Remove(course);

            try
            {
                await _context.SaveChangesAsync();
                return true;
            }
            catch (DbUpdateException ex)
            {
                // TODO: log lại nếu cần
                Console.WriteLine($"Lỗi khi xóa course: {ex.Message}");
                return false;
            }
        }

        public async Task<Course?> UpdateCourseAsync(UpdateCourseDto dto)
        {
            var course = await _context.Courses
                .Include(c => c.Departments)
                .FirstOrDefaultAsync(c => c.CourseId == dto.CourseId);

            if (course == null)
                return null;

            // ✅ Cập nhật các thuộc tính cơ bản
            course.CourseName = dto.CourseName.Trim();
            course.Description = dto.Description;
            course.CourseCategoryId = dto.CourseCategoryId;
            course.Duration = dto.Duration;
            course.Level = dto.Level;
            course.Status = dto.Status ?? course.Status;
            course.UpdatedDate = DateTime.UtcNow;

            // ✅ Cập nhật Departments (nếu có)
            if (dto.Departments != null)
            {
                // Lấy danh sách phòng ban hiện có
                var existingDepartments = course.Departments.ToList();

                // Nạp lại danh sách phòng ban mới từ DB
                var newDepartments = await _context.Departments
                    .Where(d => dto.Departments.Contains(d.Id))
                    .ToListAsync();

                // Xóa phòng ban cũ không còn được chọn
                foreach (var oldDept in existingDepartments)
                {
                    if (!newDepartments.Any(nd => nd.Id == oldDept.Id))
                        course.Departments.Remove(oldDept);
                }

                // Thêm phòng ban mới chưa có
                foreach (var newDept in newDepartments)
                {
                    if (!course.Departments.Any(d => d.Id == newDept.Id))
                        course.Departments.Add(newDept);
                }
            }

            await _context.SaveChangesAsync();
            return course;
        }


        public bool ToggleStatus(int id, string status)
        {
            var course = _context.Courses.Find(id);
            if (course == null) return false;
            if (course.Status.ToLower().Equals(CourseConstants.Status.Approve.ToLower())){
                course.Status = status;
                course.UpdatedDate = DateTime.UtcNow;
                return _context.SaveChanges() > 0;
            }
            return false;
        }
        public async Task<PagedResult<CourseListItemDto>> SearchAsync(CourseSearchRequest req, CancellationToken ct = default)
        {
            var page = req.Page <= 0 ? 1 : req.Page;
            var pageSize = req.PageSize is < 1 or > 200 ? 20 : req.PageSize;

            IQueryable<Course> q = _context.Courses
                .Include(c => c.CourseCategory)
                .Include(c => c.Departments)
                .Include(c => c.CreatedBy)
                .AsNoTracking();

            if (!string.IsNullOrWhiteSpace(req.Q))
            {
                var k = req.Q.Trim().ToLowerInvariant();
                q = q.Where(c =>
                    c.CourseName.ToLower().Contains(k) ||
                    (c.Description != null && c.Description.ToLower().Contains(k)));
            }

            if (!string.IsNullOrWhiteSpace(req.Category))
            {
                var cat = req.Category.Trim();
                var catUpper = cat.ToUpper();
                q = q.Where(c => c.CourseCategory.CategoryName.ToUpper() == catUpper);
            }

            if (req.Categories != null && req.Categories.Count > 0)
            {
                var set = req.Categories
                    .Where(s => !string.IsNullOrWhiteSpace(s))
                    .Select(s => s.Trim().ToUpper())
                    .ToHashSet();

                q = q.Where(c => set.Contains(c.CourseCategory.CategoryName.ToUpper()));
            }

            // Filters
            if (req.CategoryId.HasValue)
                q = q.Where(c => c.CourseCategoryId == req.CategoryId.Value);

            if (req.IsActive.HasValue)
                q = q.Where(c => c.Status == Constants.CourseConstants.Status.Active == req.IsActive.Value);

            if (!string.IsNullOrWhiteSpace(req.Level))
                q = q.Where(c => c.Level == req.Level);

            if (req.DurationFrom.HasValue)
                q = q.Where(c => c.Duration >= req.DurationFrom.Value);

            if (req.DurationTo.HasValue)
                q = q.Where(c => c.Duration <= req.DurationTo.Value);

            if (req.CreatedFrom.HasValue)
                q = q.Where(c => c.CreatedDate >= req.CreatedFrom.Value);

            if (req.CreatedTo.HasValue)
                q = q.Where(c => c.CreatedDate <= req.CreatedTo.Value);

            // Sorting
            q = ApplySort(q, req.Sort);

            // Total + Page
            var total = await q.CountAsync(ct);

            var items = await q
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(c => new CourseListItemDto
                {
                    Id = c.CourseId,
                    CourseId = c.CourseId,
                    CourseName = c.CourseName,
                    Code = c.Code,
                    Description = c.Description,
                    Duration = c.Duration,
                    Level = c.Level,
                    Category = c.CourseCategory.CategoryName,
                    CategoryName = c.CourseCategory.CategoryName,
                    IsActive = c.Status == CourseConstants.Status.Active,
                    IsOnline = c.IsOnline,
                    IsMandatory = c.IsMandatory,
                    CreatedDate = c.CreatedDate,
                    Status = c.Status,
                    Departments = c.Departments.Select(d => new DepartmentDto
                    {
                        DepartmentId = d.Id,
                        DepartmentName = d.Name
                    }).ToList(),
                    CreatedBy = c.CreatedBy != null ? c.CreatedBy.UserName : string.Empty,
                    UpdatedDate = c.UpdatedDate,
                    UpdatedBy = string.Empty // Có thể thêm logic để lấy thông tin người cập nhật nếu cần
                })
                .ToListAsync(ct);

            return new PagedResult<CourseListItemDto>
            {
                Items = items,
                TotalCount = total,
                Page = page,
                PageSize = pageSize
            };
        }
        private static IQueryable<Course> ApplySort(IQueryable<Course> q, string? sort)
        {
            return sort switch
            {
                "CourseName" => q.OrderBy(c => c.CourseName),
                "-CourseName" => q.OrderByDescending(c => c.CourseName),
                "Duration" => q.OrderBy(c => c.Duration),
                "-Duration" => q.OrderByDescending(c => c.Duration),
                "CreatedDate" => q.OrderBy(c => c.CreatedDate),
                "-CreatedDate" => q.OrderByDescending(c => c.CreatedDate),
                "Level" => q.OrderBy(c => c.Level),
                "-Level" => q.OrderByDescending(c => c.Level),
                _ => q.OrderByDescending(c => c.CreatedDate) // default
            };
        }

        public Course? GetCourseByCourseID(int? couseId)
        {
            return _context.Courses.FirstOrDefault(c => c.CourseId == couseId);
        }

        public async Task<IEnumerable<CourseListDto>> GetAllCoursesAsync()
        {
            return await _context.Courses
                .Include(c => c.CourseCategory)
                .Include(c=>c.Departments)
                .Where(c => c.Status==CourseConstants.Status.Active)
                .OrderByDescending(c => c.CreatedDate)
                .Select(c => new CourseListDto
                {
                    CourseId = c.CourseId,
                    CourseName = c.CourseName,
                    Description = c.Description,
                    Duration = c.Duration,
                    Level = c.Level,
                    CategoryName = c.CourseCategory.CategoryName,
                    Status = CourseConstants.Status.Active,
                    CreatedDate = c.CreatedDate,
                    Departments = c.Departments.Select(d => new DepartmentDto{
                        DepartmentId = d.Id,
                        DepartmentName = d.Name }).ToList()})
                .ToListAsync();
        }

        public async Task<PagedResult<CourseListItemDto>> GetAllCoursesPagedAsync(GetAllCoursesRequest request)
        {
            var query = _context.Courses
                .Include(c => c.CourseCategory)
                .Include(c => c.Departments)
                .Include(c => c.CreatedBy)
                .AsQueryable();

            // Apply search filter
            if (!string.IsNullOrWhiteSpace(request.Search))
            {
                var searchTerm = request.Search.Trim().ToLower();
                query = query.Where(c => 
                    c.CourseName.ToLower().Contains(searchTerm) ||
                    (c.Description != null && c.Description.ToLower().Contains(searchTerm)) ||
                    c.CourseCategory.CategoryName.ToLower().Contains(searchTerm));
            }

            // Apply status filter
            if (!string.IsNullOrWhiteSpace(request.Status))
            {
                query = query.Where(c => c.Status == request.Status);
            }

            // Get total count
            var totalCount = await query.CountAsync();

            // Apply pagination and ordering
            var items = await query
                .OrderByDescending(c => c.CreatedDate)
                .Skip((request.Page - 1) * request.PageSize)
                .Take(request.PageSize)
                .Select(c => new CourseListItemDto
                {
                    Id = c.CourseId,
                    CourseId = c.CourseId,
                    CourseName = c.CourseName,
                    Code = c.Code,
                    Description = c.Description,
                    Duration = c.Duration,
                    Level = c.Level,
                    Category = c.CourseCategory.CategoryName,
                    CategoryName = c.CourseCategory.CategoryName,
                    IsActive = c.Status == CourseConstants.Status.Active,
                    IsOnline = c.IsOnline,
                    IsMandatory = c.IsMandatory,
                    CreatedDate = c.CreatedDate,
                    Status = c.Status,
                    Departments = c.Departments.Select(d => new DepartmentDto
                    {
                        DepartmentId = d.Id,
                        DepartmentName = d.Name
                    }).ToList(),
                    CreatedBy = c.CreatedBy != null ? c.CreatedBy.UserName : string.Empty,
                    UpdatedDate = c.UpdatedDate,
                    UpdatedBy = string.Empty // Có thể thêm logic để lấy thông tin người cập nhật nếu cần
                })
                .ToListAsync();

            return new PagedResult<CourseListItemDto>
            {
                Items = items,
                TotalCount = totalCount,
                Page = request.Page,
                PageSize = request.PageSize
            };
        }

        public async Task<IEnumerable<CourseListDto>> GetCoursesByIdentifiersAsync(List<string> identifiers)
        {
            if (identifiers == null || !identifiers.Any())
            {
                return new List<CourseListDto>();
            }

            var courseIds = new List<int>();
            var courseNames = new List<string>();

            foreach (var identifier in identifiers)
            {
                if (int.TryParse(identifier, out int courseId))
                {
                    courseIds.Add(courseId);
                }
                else
                {
                    courseNames.Add(identifier);
                }
            }

            return await _context.Courses
                .Include(c => c.CourseCategory)
                .Where(c => courseIds.Contains(c.CourseId) ||
                           courseNames.Any(name => c.CourseName.ToLower().Contains(name.ToLower())))
                .OrderByDescending(c => c.CreatedDate)
                .Select(c => new CourseListDto
                {
                    CourseId = c.CourseId,
                    CourseName = c.CourseName,
                    Description = c.Description,
                    Duration = c.Duration,
                    Level = c.Level,
                    CategoryName = c.CourseCategory.CategoryName,
                    Status = c.Status,
                    CreatedDate = c.CreatedDate,
                    Departments = c.Departments.Select(d => new DepartmentDto
                    {
                        DepartmentId = d.Id,
                        DepartmentName = d.Name
                    }).ToList()
                })
                .ToListAsync();
        }

        public async Task<CourseDetailDto?> GetCourseDetailAsync(int courseId)
        {
            var course = await _context.Courses
                .Include(c => c.CourseCategory)
                .Include(c => c.Departments)
                .Include(c => c.CourseEnrollments)
                .FirstOrDefaultAsync(c => c.CourseId == courseId);

            if (course == null)
            {
                return null;
            }

            // Calculate enrollment count
            var enrollmentCount = course.CourseEnrollments?.Count ?? 0;

            // For now, we'll use a default rating of 4.5. 
            // In the future, this should be calculated from actual ratings

            return new CourseDetailDto
            {
                CourseId = course.CourseId,
                CourseName = course.CourseName,
                Description = course.Description,
                Duration = course.Duration,
                Level = course.Level,
                CategoryName = course.CourseCategory?.CategoryName ?? "Unknown",
                CategoryId = course.CourseCategoryId,
                Status = course.Status,
                CreatedDate = course.CreatedDate,
                UpdatedDate = course.UpdatedDate,
                Prerequisites = null, // Not available in current model
                Objectives = null, // Not available in current model
                Price = null, // Not available in current model
                EnrollmentCount = enrollmentCount,
                AverageRating = AverageRatingPass,
                Departments = course.Departments.Select(d => new DepartmentDto
                {
                    DepartmentId = d.Id,
                    DepartmentName = d.Name
                }).ToList()
            };
        }

        //Hiển thị các course có status là Pending-ban giám đốc
        public async Task<IEnumerable<CourseListDto>> GetPendingCoursesAsync()
        {
            return await _context.Courses
                .Include(c => c.CourseCategory)
                .Include(c => c.Departments)
                .Where(c => c.Status == CourseConstants.Status.Pending)
                .OrderByDescending(c => c.CreatedDate)
                .Select(c => new CourseListDto
                {
                    CourseId = c.CourseId,
                    CourseName = c.CourseName,
                    Description = c.Description,
                    Duration = c.Duration,
                    Level = c.Level,
                    CategoryName = c.CourseCategory.CategoryName,
                    Status = c.Status,
                    CreatedDate = c.CreatedDate,
                    Departments = c.Departments.Select(d => new DepartmentDto
                    {
                        DepartmentId = d.Id,
                        DepartmentName = d.Name
                    }).ToList()
                })
                .ToListAsync();
        }

        // Duyệt khóa học - ban giám đốc
        public async Task<bool> UpdatePendingCourseStatusAsync(int courseId, string newStatus)
        {
            if (string.IsNullOrWhiteSpace(newStatus))
                throw new ArgumentException("Trạng thái mới không hợp lệ.", nameof(newStatus));

            var allowedStatuses = new[]
            {
                CourseConstants.Status.Approve,
                CourseConstants.Status.Reject
                };

            if (!allowedStatuses.Contains(newStatus, StringComparer.OrdinalIgnoreCase))
                throw new ArgumentException($"Trạng thái '{newStatus}' không hợp lệ. Chỉ chấp nhận Approve hoặc Reject.");

            var course = await _context.Courses
                .Include(c => c.Departments)
                .FirstOrDefaultAsync(c => c.CourseId == courseId);

            if (course == null)
                return false;

            // Chỉ cho phép xử lý khi đang Pending
            if (!course.Status.Equals(CourseConstants.Status.Pending, StringComparison.OrdinalIgnoreCase))
                return false;

            if (newStatus.Equals(CourseConstants.Status.Approve, StringComparison.OrdinalIgnoreCase))
            {
                // ✅ Duyệt khóa học
                course.Status = CourseConstants.Status.Approve;
                course.UpdatedDate = DateTime.UtcNow;
            }

            await _context.SaveChangesAsync();
            return true;
        }

        //Update reject conmeback
        public async Task<bool> UpdateDraftAndResubmitAsync(int courseId, UpdateCourseRejectDto dto)
        {
            // 1) Validate input cơ bản
            if (dto is null) throw new ArgumentNullException(nameof(dto));
            if (string.IsNullOrWhiteSpace(dto.CourseName))
                throw new ArgumentException("Tên khóa học không được để trống.", nameof(dto.CourseName));

            // 2) Tải course + Departments
            var course = await _context.Courses
                .Include(c => c.Departments)
                .FirstOrDefaultAsync(c => c.CourseId == courseId);

            if (course == null) return false;

            // 3) Chỉ cho phép sửa và "gửi lại" khi đang Pending (bản nháp) hoặc Reject
            var canResubmit =
                course.Status.Equals(CourseConstants.Status.Pending, StringComparison.OrdinalIgnoreCase) ||
                course.Status.Equals(CourseConstants.Status.Reject, StringComparison.OrdinalIgnoreCase);

            if (!canResubmit) return false;

            // 4) Cập nhật field nội dung
            course.CourseName = dto.CourseName.Trim();
            course.Description = dto.Description?.Trim();
            course.Duration = dto.Duration;
            course.Level = dto.Level.Trim();
            course.CourseCategoryId = dto.CourseCategoryId;

            // 5) Đồng bộ Departments (many-to-many) theo danh sách ID được gửi lên
            //    - Lấy các Department hiện hữu
            var deps = dto.DepartmentIds?.Distinct().ToList() ?? new List<int>();
            var existingDepartments = deps.Count == 0
                ? new List<Department>()
                : await _context.Departments.Where(d => deps.Contains(d.Id)).ToListAsync();

            //    - Gán lại tập Departments để EF Core tự sync (add/remove join rows)
            course.Departments = existingDepartments;

            // 6) Đưa trạng thái về Pending để "gửi lại", cập nhật thời gian
            course.Status = CourseConstants.Status.Pending;
            course.UpdatedDate = DateTime.UtcNow;

            await _context.SaveChangesAsync();
            return true;
        }

        // Ban giám đốc xóa khóa học đã duyệt
        public async Task<bool> DeleteActiveCourseAsync(int courseId)
        {
            var course = await _context.Courses.FirstOrDefaultAsync(c => c.CourseId == courseId);
            if (course == null)
                return false;

            // Chỉ cho phép cập nhật nếu course đang Active
            if (!course.Status.Equals(CourseConstants.Status.Active, StringComparison.OrdinalIgnoreCase))
                return false;

            course.Status = CourseConstants.Status.Deleted;
            course.UpdatedDate = DateTime.UtcNow;

            await _context.SaveChangesAsync();
            return true;
        }

    }
}
