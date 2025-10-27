using ClosedXML.Excel;
using InternalTrainingSystem.Core.Configuration;
using InternalTrainingSystem.Core.Constants;
using InternalTrainingSystem.Core.DB;
using InternalTrainingSystem.Core.DTOs;
using InternalTrainingSystem.Core.Models;
using InternalTrainingSystem.Core.Services.Interface;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace InternalTrainingSystem.Core.Services.Implement
{
    public class CourseService : ICourseService
    {
        private readonly ApplicationDbContext _context;
        private readonly IFileStorage _storage;
        private readonly ICourseMaterialService _courseMaterialService;
        private const double AverageRatingPass = 4.5;

        public CourseService(ApplicationDbContext context, IFileStorage storage,
        ICourseMaterialService courseMaterialService)
        {
            _context = context;
            _storage = storage;
            _courseMaterialService = courseMaterialService;
        }

        // Hàm lấy ra Course theo code
        public async Task<Course?> GetCourseByCourseCodeAsync(string courseCode)
        {
            if (string.IsNullOrWhiteSpace(courseCode))
                throw new ArgumentException("Mã khóa học không hợp lệ.", nameof(courseCode));

            // Chuẩn hóa mã trước khi tìm (tránh lỗi khoảng trắng / hoa thường)
            var normalizedCode = courseCode.Trim().ToLower();

            return await _context.Courses
                .Include(c => c.CourseCategory)
                .Include(c => c.Departments)
                .Include(c => c.CreatedBy)
                .FirstOrDefaultAsync(c => c.Code.ToLower() == normalizedCode);
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

            course.CourseName = dto.CourseName.Trim();
            course.Description = dto.Description;
            course.CourseCategoryId = dto.CourseCategoryId;
            course.Duration = dto.Duration;
            course.Level = dto.Level;
            course.Status = dto.Status ?? course.Status;
            course.UpdatedDate = DateTime.UtcNow;

            if (dto.Departments != null)
            {
                var existingDepartments = course.Departments.ToList();

                var newDepartments = await _context.Departments
                    .Where(d => dto.Departments.Contains(d.Id))
                    .ToListAsync();

                foreach (var oldDept in existingDepartments)
                {
                    if (!newDepartments.Any(nd => nd.Id == oldDept.Id))
                        course.Departments.Remove(oldDept);
                }

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
                q = q.Where(c => c.Status == CourseConstants.Status.Approve);

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
                    IsActive = c.Status == CourseConstants.Status.Approve,
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
                    IsActive = c.Status == CourseConstants.Status.Approve,
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

        public async Task<IEnumerable<CourseListItemDto>> GetCoursesByIdentifiersAsync(List<string> identifiers)
        {
            if (identifiers == null || !identifiers.Any())
            {
                return new List<CourseListItemDto>();
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
                .Select(c => new CourseListItemDto
                {
                    CourseId = c.CourseId,
                    CourseName = c.CourseName,
                    Description = c.Description,
                    Duration = c.Duration,
                    Level = c.Level,
                    CategoryName = c.CourseCategory.CategoryName,
                    Status = c.Status,
                    IsActive = c.Status == CourseConstants.Status.Approve,
                    IsOnline = c.IsOnline,
                    IsMandatory = c.IsMandatory,
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
                Code = course.Code,
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
                Departments = course.Departments.Select(d => new DepartmentDto
                {
                    DepartmentId = d.Id,
                    DepartmentName = d.Name
                }).ToList()
            };
        }

        //Hiển thị các course có status là Pending-ban giám đốc
        public async Task<IEnumerable<CourseListItemDto>> GetPendingCoursesAsync()
        {
            return await _context.Courses
                .Include(c => c.CourseCategory)
                .Include(c => c.Departments)
                .Where(c => c.Status == CourseConstants.Status.Pending)
                .OrderByDescending(c => c.CreatedDate)
                .Select(c => new CourseListItemDto
                {
                    CourseId = c.CourseId,
                    CourseName = c.CourseName,
                    Description = c.Description,
                    Duration = c.Duration,
                    Level = c.Level,
                    CategoryName = c.CourseCategory.CategoryName,
                    Status = c.Status,
                    IsActive = c.Status == CourseConstants.Status.Approve,
                    IsOnline = c.IsOnline,
                    IsMandatory = c.IsMandatory,
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
        public async Task<bool> DeleteActiveCourseAsync(int courseId, string rejectReason)
        {
            var course = await _context.Courses.FirstOrDefaultAsync(c => c.CourseId == courseId);
            if (course == null)
                return false;

            // Chỉ cho phép xóa nếu khóa học đang Active
            if (!course.Status.Equals(CourseConstants.Status.Approve, StringComparison.OrdinalIgnoreCase))
                return false;

            // Cập nhật trạng thái và lý do từ chối
            course.Status = CourseConstants.Status.Draft;
            course.RejectionReason = string.IsNullOrWhiteSpace(rejectReason)
                ? "Khóa học bị xóa bởi Ban giám đốc."
                : rejectReason.Trim();

            course.UpdatedDate = DateTime.UtcNow;

            await _context.SaveChangesAsync();
            return true;
        }
        public async Task<Course> CreateFullCourseAsync(CreateFullCourseMetadataDto meta,
                                IList<IFormFile> lessonFiles, string createdByUserId, CancellationToken ct = default)
        {
            await using var tx = await _context.Database.BeginTransactionAsync(ct);
            try
            {
                var course = new Course
                {
                    Code = meta.CourseCode.Trim(),
                    CourseName = meta.CourseName.Trim(),
                    Description = meta.Description,
                    CourseCategoryId = meta.CourseCategoryId,
                    Duration = meta.Duration,
                    Level = meta.Level,
                    Status = CourseConstants.Status.Pending,
                    CreatedDate = DateTime.UtcNow,
                    IsOnline = meta.IsOnline,
                    IsMandatory = meta.IsMandatory,
                    CreatedById = createdByUserId
                };

                if (meta.Departments?.Any() == true)
                {
                    var deps = await _context.Departments
                        .Where(d => meta.Departments.Contains(d.Id))
                        .ToListAsync(ct);

                    if (deps.Count != meta.Departments.Count)
                        throw new ArgumentException("Some DepartmentIds are invalid");

                    foreach (var d in deps)
                        course.Departments.Add(d);
                }

                await _context.Courses.AddAsync(course, ct);
                await _context.SaveChangesAsync(ct); 

                var fileCursor = 0;

                foreach (var modSpec in meta.Modules.OrderBy(m => m.OrderIndex))
                {
                    var module = new CourseModule
                    {
                        CourseId = course.CourseId,
                        Title = modSpec.Title.Trim(),
                        Description = modSpec.Description,
                        OrderIndex = modSpec.OrderIndex
                    };

                    await _context.CourseModules.AddAsync(module, ct);
                    await _context.SaveChangesAsync(ct); 

                    foreach (var lessonSpec in modSpec.Lessons.OrderBy(l => l.OrderIndex))
                    {
                        Lesson newLesson;

                        if (lessonSpec.Type == LessonType.Quiz && lessonSpec.IsQuizExcel)
                        {
                            if (fileCursor >= lessonFiles.Count)
                                throw new ArgumentException("Thiếu file Excel cho lesson Quiz.");

                            var excelFile = lessonFiles[fileCursor++];

                            var quizId = await ImportQuizFromExcelInternal(
                                course.CourseId,
                                lessonSpec.QuizTitle ?? lessonSpec.Title,
                                excelFile,
                                ct
                            );

                            newLesson = new Lesson
                            {
                                ModuleId = module.Id,
                                Title = lessonSpec.Title.Trim(),
                                Type = LessonType.Quiz,
                                OrderIndex = lessonSpec.OrderIndex,
                                QuizId = quizId
                            };

                            await _context.Lessons.AddAsync(newLesson, ct);
                            await _context.SaveChangesAsync(ct);

                            continue;
                        }

                        if (lessonSpec.UploadBinary &&
                            (lessonSpec.Type == LessonType.File ||
                             lessonSpec.Type == LessonType.Video ||
                             lessonSpec.Type == LessonType.Reading))
                        {
                            if (fileCursor >= lessonFiles.Count)
                                throw new ArgumentException("Thiếu file binary cho lesson UploadBinary=true.");

                            var binFile = lessonFiles[fileCursor++];

                            newLesson = new Lesson
                            {
                                ModuleId = module.Id,
                                Title = lessonSpec.Title.Trim(),
                                Type = lessonSpec.Type,
                                OrderIndex = lessonSpec.OrderIndex,
                                ContentHtml = lessonSpec.ContentHtml
                            };

                            await _context.Lessons.AddAsync(newLesson, ct);
                            await _context.SaveChangesAsync(ct); 

                            await _courseMaterialService.UploadLessonBinaryAsync(newLesson.Id, binFile, ct);

                            continue;
                        }

                        newLesson = new Lesson
                        {
                            ModuleId = module.Id,
                            Title = lessonSpec.Title.Trim(),
                            Type = lessonSpec.Type,
                            OrderIndex = lessonSpec.OrderIndex,
                            ContentUrl = lessonSpec.ContentUrl,
                            ContentHtml = lessonSpec.ContentHtml
                        };

                        await _context.Lessons.AddAsync(newLesson, ct);
                        await _context.SaveChangesAsync(ct);
                    }
                }

                await tx.CommitAsync(ct);
                return course;
            }
            catch
            {
                await tx.RollbackAsync(ct);
                throw;
            }
        }
        private async Task<int> ImportQuizFromExcelInternal(int courseId, string quizTitle,
                                                            IFormFile excelFile, CancellationToken ct)
        {
            if (excelFile == null || excelFile.Length == 0)
                throw new ArgumentException("Quiz Excel file is empty.");

            using var stream = excelFile.OpenReadStream();
            using var workbook = new XLWorkbook(stream);
            var worksheet = workbook.Worksheets.FirstOrDefault();
            if (worksheet == null)
                throw new ArgumentException("Invalid Excel format: no worksheet found.");

            var quiz = new Quiz
            {
                CourseId = courseId,
                Title = quizTitle,
                Description = $"Imported from {excelFile.FileName}",
                TimeLimit = 30,
                MaxAttempts = 3,
                PassingScore = 70,
                IsActive = true,
                CreatedDate = DateTime.UtcNow
            };

            _context.Quizzes.Add(quiz);
            await _context.SaveChangesAsync(ct);

            int row = 2; 
            int order = 1;

            while (!worksheet.Row(row).IsEmpty())
            {
                var qText = worksheet.Cell(row, 1).GetString().Trim();
                if (string.IsNullOrWhiteSpace(qText))
                {
                    row++;
                    continue;
                }

                var qType = worksheet.Cell(row, 2).GetString().Trim();
                var points = worksheet.Cell(row, 3).GetValue<int?>() ?? 1;

                var question = new Question
                {
                    QuizId = quiz.QuizId,
                    QuestionText = qText,
                    QuestionType = string.IsNullOrWhiteSpace(qType)
                        ? QuizConstants.QuestionTypes.MultipleChoice
                        : qType,
                    Points = points,
                    OrderIndex = order++,
                    IsActive = true,
                    CreatedDate = DateTime.UtcNow
                };

                _context.Questions.Add(question);
                await _context.SaveChangesAsync(ct);

                var answers = new List<Answer>();
                int col = 4;
                int answerIndex = 1;
                while (true)
                {
                    var answerText = worksheet.Cell(row, col).GetString();
                    if (string.IsNullOrWhiteSpace(answerText))
                        break;

                    var isCorrect = false;
                    var correctCol = col + 1;
                    if (!worksheet.Cell(row, correctCol).IsEmpty())
                    {
                        var rawVal = worksheet.Cell(row, correctCol).GetString().Trim().ToLower();
                        isCorrect = rawVal is "true" or "1" or "x" or "yes";
                    }

                    answers.Add(new Answer
                    {
                        QuestionId = question.QuestionId,
                        AnswerText = answerText.Trim(),
                        IsCorrect = isCorrect,
                        OrderIndex = answerIndex++,
                        IsActive = true
                    });

                    col += 2;
                }

                if (answers.Count == 0)
                {
                    answers.Add(new Answer
                    {
                        QuestionId = question.QuestionId,
                        AnswerText = "(Essay answer)",
                        IsCorrect = true,
                        OrderIndex = 1
                    });
                }

                _context.Answers.AddRange(answers);
                await _context.SaveChangesAsync(ct);

                row++;
            }

            return quiz.QuizId;
        }

    }
}
