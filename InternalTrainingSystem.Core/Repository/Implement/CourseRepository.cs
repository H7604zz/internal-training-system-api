using ClosedXML.Excel;
using InternalTrainingSystem.Core.Common;
using InternalTrainingSystem.Core.Common.Constants;
using InternalTrainingSystem.Core.DB;
using InternalTrainingSystem.Core.DTOs;
using InternalTrainingSystem.Core.Models;
using InternalTrainingSystem.Core.Repository.Interface;
using InternalTrainingSystem.Core.Services.Implement;
using InternalTrainingSystem.Core.Services.Interface;
using Microsoft.EntityFrameworkCore;

namespace InternalTrainingSystem.Core.Repository.Implement
{
    public class CourseRepository : ICourseRepository
    {
        private readonly ApplicationDbContext _context;
        private readonly IFileStorage _storage;
        private readonly ICourseMaterialRepository _courseMaterialRepo;
        private const double AverageRatingPass = 4.5;

        public CourseRepository(ApplicationDbContext context, IFileStorage storage,
        ICourseMaterialRepository courseMaterialRepo)
        {
            _context = context;
            _storage = storage;
            _courseMaterialRepo = courseMaterialRepo;
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

        public async Task<Course> UpdateCourseAsync(int courseId,UpdateCourseMetadataDto meta,IList<IFormFile> lessonFiles,string updatedByUserId, CancellationToken ct = default)
        {
            await using var tx = await _context.Database.BeginTransactionAsync(ct);

            try
            {
                var course = await _context.Courses
            .Include(c => c.Modules)
                .ThenInclude(m => m.Lessons)
            .Include(c => c.Departments)
            .FirstOrDefaultAsync(c => c.CourseId == courseId, ct);

                if (course == null)
                    throw new ArgumentException($"Course ID {courseId} not found.");

                // 1) Cập nhật thông tin cơ bản
                course.CourseName = meta.CourseName.Trim();
                course.Description = meta.Description?.Trim();
                course.CourseCategoryId = meta.CourseCategoryId;
                course.Duration = meta.Duration;
                course.Level = meta.Level;
                course.IsOnline = meta.IsOnline;
                course.IsMandatory = meta.IsMandatory;
                course.UpdatedDate = DateTime.Now;
                course.CreatedById = updatedByUserId;

                // 2) Departments
                course.Departments.Clear();
                if (meta.Departments?.Any() == true)
                {
                    var deps = await _context.Departments.Where(d => meta.Departments.Contains(d.Id)).ToListAsync(ct);

                    if (deps.Count != meta.Departments.Count)
                        throw new ArgumentException("Some DepartmentIds are invalid");

                    foreach (var d in deps)
                        course.Departments.Add(d);
                }

                // 3) Modules (xóa những cái không còn)
                var existingModules = course.Modules.ToList();
                var metaModules = meta.Modules.OrderBy(m => m.OrderIndex).ToList();

                foreach (var oldMod in existingModules)
                {
                    // meta chỉ giữ lại module có ModuleId trùng; module mới có ModuleId = null
                    var stillExists = metaModules.Any(m => m.ModuleId == oldMod.Id);
                    if (!stillExists)
                    {
                        // xóa luôn lessons liên quan (cascade nếu đã cấu hình)
                        _context.CourseModules.Remove(oldMod);
                    }
                }

                await _context.SaveChangesAsync(ct);

                // 4) Thêm/cập nhật modules & lessons
                foreach (var modSpec in metaModules)
                {
                    CourseModule module =
                        modSpec.ModuleId.HasValue
                        ? existingModules.FirstOrDefault(m => m.Id == modSpec.ModuleId.Value)!
                        : null;

                    if (module == null)
                    {
                        // Thêm mới
                        module = new CourseModule
                        {
                            CourseId = course.CourseId,
                            Title = modSpec.Title.Trim(),
                            Description = modSpec.Description,
                            OrderIndex = modSpec.OrderIndex
                        };
                        await _context.CourseModules.AddAsync(module, ct);
                        await _context.SaveChangesAsync(ct); // cần Id
                    }
                    else
                    {
                        // Cập nhật
                        module.Title = modSpec.Title.Trim();
                        module.Description = modSpec.Description;
                        module.OrderIndex = modSpec.OrderIndex;
                        await _context.SaveChangesAsync(ct);
                    }

                    // Đồng bộ lessons của module này
                    var existingLessons = module.Lessons.ToList();
                    var metaLessons = modSpec.Lessons.OrderBy(l => l.OrderIndex).ToList();

                    // Xóa lessons không còn nữa
                    foreach (var oldLesson in existingLessons)
                    {
                        var stillExists = metaLessons.Any(l => l.LessonId == oldLesson.Id);
                        if (!stillExists) _context.Lessons.Remove(oldLesson);
                    }
                    await _context.SaveChangesAsync(ct);

                    // Thêm/cập nhật từng lesson
                    foreach (var lessonSpec in metaLessons)
                    {
                        Lesson lesson =
                            lessonSpec.LessonId.HasValue
                            ? existingLessons.FirstOrDefault(l => l.Id == lessonSpec.LessonId.Value)!
                            : null;

                        if (lesson == null)
                        {
                            // Thêm mới
                            lesson = new Lesson
                            {
                                ModuleId = module.Id,
                                Title = lessonSpec.Title.Trim(),
                                Description = lessonSpec.Description,
                                Type = lessonSpec.Type,
                                OrderIndex = lessonSpec.OrderIndex,
                                AttachmentUrl = lessonSpec.AttachmentUrl,
                            };
                            await _context.Lessons.AddAsync(lesson, ct);
                            await _context.SaveChangesAsync(ct);
                        }
                        else
                        {
                            // Cập nhật
                            lesson.Title = lessonSpec.Title.Trim();
                            lesson.Description = lessonSpec.Description;
                            lesson.Type = lessonSpec.Type;
                            lesson.OrderIndex = lessonSpec.OrderIndex;
                            lesson.AttachmentUrl = lessonSpec.AttachmentUrl;
                            await _context.SaveChangesAsync(ct);
                        }

                        // Upload file mới (nếu có)
                        if (lessonSpec.MainFileIndex is not null)
                        {
                            var idx = lessonSpec.MainFileIndex.Value;
                            if (idx < 0 || idx >= lessonFiles.Count)
                                throw new ArgumentException($"MainFileIndex {idx} out of range for lesson '{lessonSpec.Title}'.");

                            await _courseMaterialRepo.UploadLessonBinaryAsync(lesson.Id, lessonFiles[idx],ct);
                        }

                        // Quiz (Excel)
                        if (lessonSpec.Type == LessonType.Quiz && lessonSpec.IsQuizExcel)
                        {
                            var timeLimit = lessonSpec.QuizTimeLimit ?? 30;
                            var maxAttempts = lessonSpec.QuizMaxAttempts ?? 3;
                            var passing = lessonSpec.QuizPassingScore ?? 70;
                            if (lessonSpec.MainFileIndex is null)
                                throw new ArgumentException($"Lesson '{lessonSpec.Title}' requires Excel file.");

                            var excelFile = lessonFiles[lessonSpec.MainFileIndex.Value];
                            var quizId = await ImportQuizFromExcelInternal(
                                course.CourseId,
                                lessonSpec.QuizTitle ?? lessonSpec.Title,
                                timeLimit,
                                maxAttempts,
                                passing,
                                excelFile,ct);
                            lesson.QuizId = quizId;
                            await _context.SaveChangesAsync(ct);
                        }

                        // Video (URL) – đảm bảo ContentUrl có giá trị
                        if (lessonSpec.Type == LessonType.Video)
                        {
                            if (string.IsNullOrWhiteSpace(lessonSpec.ContentUrl))
                                throw new ArgumentException($"Lesson '{lessonSpec.Title}' is Video but ContentUrl is empty.");
                            lesson.ContentUrl = lessonSpec.ContentUrl;
                            await _context.SaveChangesAsync(ct);
                        }
                    }
                }

                await _context.SaveChangesAsync(ct);
                await tx.CommitAsync(ct);
                return course;
            }
            catch
            {
                await tx.RollbackAsync(ct);
                throw;
            }
        }

        public bool ToggleStatus(int id, string status)
        {
            var course = _context.Courses.Find(id);
            if (course == null) return false;
            if (course.Status.ToLower().Equals(CourseConstants.Status.Approve.ToLower()))
            {
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
                    Departments = c.Departments.Select(d => new DepartmentListDto
                    {
                        DepartmentId = d.Id,
                        DepartmentName = d.Name,
                        Description = d.Description,
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

        public async Task<Course?> GetCourseByCourseIdAsync(int? couseId)
        {
            return await _context.Courses
                .Include(c => c.CourseCategory)
                .Include(c => c.Departments)
                .Include(c => c.CreatedBy)
                .FirstOrDefaultAsync(c => c.CourseId == couseId);
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
                    Departments = c.Departments.Select(d => new DepartmentListDto
                    {
                        DepartmentId = d.Id,
                        DepartmentName = d.Name,
                        Description = d.Description,
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
        public async Task<CourseDetailDto?> GetCourseDetailAsync(int courseId, CancellationToken ct = default)
        {
            var course = await _context.Courses
                .Include(c => c.CourseCategory)
                .Include(c => c.Departments)
                .Include(c => c.Modules)
                    .ThenInclude(m => m.Lessons)
                .Include(c => c.CreatedBy)
                .FirstOrDefaultAsync(c => c.CourseId == courseId, ct);

            if (course == null)
                return null;

            return new CourseDetailDto
            {
                CourseId = course.CourseId,
                Code = course.Code ?? "",
                CourseName = course.CourseName,
                Description = course.Description,
                CategoryName = course.CourseCategory.CategoryName,
                Duration = course.Duration,
                Level = course.Level,
                Status = course.Status,
                IsOnline = course.IsOnline,
                IsMandatory = course.IsMandatory,
                PassScore = course.PassScore,
                CreatedDate = course.CreatedDate,
                UpdatedDate = course.UpdatedDate,
                CreatedBy = course.CreatedBy.FullName,
                Departments = course.Departments.Select(d => new DepartmentListDto
                {
                    DepartmentId = d.Id,
                    DepartmentName = d.Name
                }).ToList(),
                Modules = course.Modules
                    .OrderBy(m => m.OrderIndex)
                    .Select(m => new ModuleDetailDto
                    {
                        Id = m.Id,
                        CourseId = m.CourseId,
                        Title = m.Title,
                        Description = m.Description,
                        OrderIndex = m.OrderIndex,
                        Lessons = m.Lessons
                            .OrderBy(l => l.OrderIndex)
                            .Select(l => new LessonListItemDto
                            {
                                Id = l.Id,
                                ModuleId = l.ModuleId,
                                Title = l.Title,
                                Description = l.Description,
                                Type = l.Type,
                                OrderIndex = l.OrderIndex,
                                ContentUrl = l.ContentUrl,
                                QuizId = l.QuizId
                            }).ToList()
                    }).ToList()
            };
        }

        public async Task<Course?> GetCourseWithDepartmentsAsync(int courseId)
        {
            return await _context.Courses
                .Include(c => c.Departments)
                .FirstOrDefaultAsync(c => c.CourseId == courseId);
        }

        public async Task AddCourseHistoryAsync(CourseHistory history)
        {
            await _context.CourseHistories.AddAsync(history);
        }

        public async Task SaveChangesAsync()
        {
            await _context.SaveChangesAsync();
        }

        


        //Update reject conmeback
        public async Task<Course> UpdateAndResubmitToPendingAsync(int courseId,UpdateCourseMetadataDto meta,IList<IFormFile> lessonFiles,string updatedByUserId,
                                                                  string? resubmitNote = null,CancellationToken ct = default)
        {
            // 1) Cập nhật nội dung (modules/lessons/files...)
            var course = await UpdateCourseAsync(courseId, meta, lessonFiles, updatedByUserId, ct);

            // 2) Đưa trạng thái về Pending để chờ duyệt lại
            //    (xóa/ghi chú lý do reject cũ tùy bạn)
            course.Status = CourseConstants.Status.Pending;
            course.RejectionReason = resubmitNote;          // hoặc null nếu bạn muốn xoá lý do cũ
            course.UpdatedDate = DateTime.UtcNow;
            course.CreatedById = updatedByUserId;        // nếu có trường này

            await _context.SaveChangesAsync(ct);
            return course;
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
        public async Task<Course> CreateCourseAsync(CreateCourseMetadataDto meta,IList<IFormFile> lessonFiles,string createdByUserId,CancellationToken ct = default)
        {
            await using var tx = await _context.Database.BeginTransactionAsync(ct);

            try
            {
                // 1. Course
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
                    CreatedById = createdByUserId,
                    PassScore = meta.PassScore
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

                // 2. Modules + Lessons
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

                        // CASE 1: QUIZ (Excel import)
                        if (lessonSpec.Type == LessonType.Quiz && lessonSpec.IsQuizExcel)
                        {
                            var timeLimit = lessonSpec.QuizTimeLimit ?? 30;
                            var maxAttempts = lessonSpec.QuizMaxAttempts ?? 3;
                            var passing = lessonSpec.QuizPassingScore ?? 70;
                            if (lessonSpec.MainFileIndex is null || lessonSpec.MainFileIndex < 0 || lessonSpec.MainFileIndex >= lessonFiles.Count)
                                throw new ArgumentException("Thiếu hoặc sai file Excel cho lesson Quiz.");

                            var excelFile = lessonFiles[lessonSpec.MainFileIndex.Value];
                            var quizId = await ImportQuizFromExcelInternal(course.CourseId, lessonSpec.QuizTitle ?? lessonSpec.Title, timeLimit, maxAttempts, passing, excelFile, ct);

                            newLesson = new Lesson
                            {
                                ModuleId = module.Id,
                                Title = lessonSpec.Title.Trim(),
                                Description = lessonSpec.Description,
                                Type = LessonType.Quiz,
                                OrderIndex = lessonSpec.OrderIndex,
                                QuizId = quizId
                            };

                            await _context.Lessons.AddAsync(newLesson, ct);
                            await _context.SaveChangesAsync(ct);
                            continue;
                        }

                        //  CASE 2: VIDEO (URL)
                        if (lessonSpec.Type == LessonType.Video)
                        {
                            if (string.IsNullOrWhiteSpace(lessonSpec.ContentUrl))
                                throw new ArgumentException($"Lesson '{lessonSpec.Title}' là Video nhưng chưa có ContentUrl.");

                            newLesson = new Lesson
                            {
                                ModuleId = module.Id,
                                Title = lessonSpec.Title.Trim(),
                                Description = lessonSpec.Description,
                                Type = LessonType.Video,
                                OrderIndex = lessonSpec.OrderIndex,
                                ContentUrl = lessonSpec.ContentUrl,
                                AttachmentUrl = lessonSpec.AttachmentUrl
                            };

                            await _context.Lessons.AddAsync(newLesson, ct);
                            await _context.SaveChangesAsync(ct);
                            continue;
                        }

                        // CASE 3: FILE/READING 
                        newLesson = new Lesson
                        {
                            ModuleId = module.Id,
                            Title = lessonSpec.Title.Trim(),
                            Description = lessonSpec.Description,
                            Type = lessonSpec.Type,
                            OrderIndex = lessonSpec.OrderIndex,
                            AttachmentUrl = lessonSpec.AttachmentUrl
                        };

                        await _context.Lessons.AddAsync(newLesson, ct);
                        await _context.SaveChangesAsync(ct);

                        // Upload file chính nếu có
                        if (lessonSpec.MainFileIndex is not null)
                        {
                            var idx = lessonSpec.MainFileIndex.Value;
                            if (idx < 0 || idx >= lessonFiles.Count)
                                throw new ArgumentException($"MainFileIndex {idx} is out of range for lesson '{lessonSpec.Title}'.");

                            var mainFile = lessonFiles[idx];
                            await _courseMaterialRepo.UploadLessonBinaryAsync(newLesson.Id, mainFile, ct);
                        }

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
        private async Task<int> ImportQuizFromExcelInternal(int courseId,
                                                            string quizTitle,
                                                            int timeLimit,
                                                            int maxAttempts,
                                                            int passingScore,
                                                            IFormFile excelFile,
                                                            CancellationToken ct)
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
                TimeLimit = timeLimit,
                MaxAttempts = maxAttempts,
                PassingScore = passingScore,
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
