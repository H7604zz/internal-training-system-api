using InternalTrainingSystem.Core.Common.Constants;
using InternalTrainingSystem.Core.DTOs;
using InternalTrainingSystem.Core.Models;
using InternalTrainingSystem.Core.Repository.Interface;
using InternalTrainingSystem.Core.Services.Interface;
using InternalTrainingSystem.Core.Utils;

namespace InternalTrainingSystem.Core.Services.Implement
{
    public class AssignmentService : IAssignmentService
    {
        private readonly IAssignmentRepository _assignmentRepo;
        private readonly IAssignmentSubmissionRepository _submissionRepo;
        private readonly ISubmissionFileRepository _fileRepo;
        private readonly IClassRepository _classRepo;
        private readonly IFileStorage _fileStorage;
        private readonly IUnitOfWork _uow;

        public AssignmentService(
            IAssignmentRepository assignmentRepo,
            IAssignmentSubmissionRepository submissionRepo,
            ISubmissionFileRepository fileRepo,
            IClassRepository classRepo,
            IFileStorage fileStorage,
            IUnitOfWork uow)
        {
            _assignmentRepo = assignmentRepo;
            _submissionRepo = submissionRepo;
            _fileRepo = fileRepo;
            _classRepo = classRepo;
            _fileStorage = fileStorage;
            _uow = uow;
        }
        public async Task<AssignmentDto> CreateAssignmentAsync(
    CreateAssignmentForm form,
    string mentorId,
    CancellationToken ct)
        {
            var canTeach = await _classRepo.IsMentorOfClassAsync(form.ClassId, mentorId, ct);
            if (!canTeach)
                throw new UnauthorizedAccessException("Bạn không có quyền tạo assignment cho lớp này.");

            string? url = null;
            string? path = null;
            string? mime = null;
            long? size = null;

            if (form.File != null)
            {
                var meta = StorageObjectMetadata.ForUpload(form.File.FileName, form.File.ContentType);
                var folder = $"assignments/class-{form.ClassId}";

                var uploaded = await _fileStorage.SaveAsync(form.File, folder, meta, ct);

                url = uploaded.url;
                path = uploaded.relativePath;
                mime = form.File.ContentType;
                size = form.File.Length;  // ⭐ SIZE ĐÃ ĐÚNG LÀ long
            }

            var entity = new Assignment
            {
                ClassId = form.ClassId,
                ScheduleId = form.ScheduleId,
                Title = form.Title,
                Description = form.Description,
                StartAt = form.StartAt,
                DueAt = form.DueAt,
                CloseAt = form.CloseAt,
                AllowLateSubmit = form.AllowLateSubmit,
                MaxSubmissions = form.MaxSubmissions,
                MaxScore = form.MaxScore,

                AttachmentUrl = url,
                AttachmentFilePath = path,
                AttachmentMimeType = mime,
                AttachmentSizeBytes = size
            };

            await _assignmentRepo.AddAsync(entity, ct);
            await _uow.SaveChangesAsync(ct);

            return new AssignmentDto
            {
                AssignmentId = entity.AssignmentId,
                ClassId = entity.ClassId,
                ScheduleId = entity.ScheduleId,
                Title = entity.Title,
                Description = entity.Description,
                StartAt = entity.StartAt,
                DueAt = entity.DueAt,
                CloseAt = entity.CloseAt,
                AllowLateSubmit = entity.AllowLateSubmit,
                MaxSubmissions = entity.MaxSubmissions,
                MaxScore = entity.MaxScore,
                AttachmentUrl = entity.AttachmentUrl
            };
        }
        public async Task<AssignmentDto> UpdateAssignmentAsync(
    int assignmentId,
    UpdateAssignmentForm form,
    string mentorId,
    CancellationToken ct)
        {
            var assignment = await _assignmentRepo.GetWithClassAsync(assignmentId, ct)
                ?? throw new ArgumentException("Assignment không tồn tại.");

            var canTeach = await _classRepo.IsMentorOfClassAsync(assignment.ClassId, mentorId, ct);
            if (!canTeach)
                throw new UnauthorizedAccessException("Bạn không có quyền sửa assignment này.");

            // UPDATE FIELDS
            assignment.Title = form.Title;
            assignment.Description = form.Description;
            assignment.StartAt = form.StartAt;
            assignment.DueAt = form.DueAt;
            assignment.CloseAt = form.CloseAt;
            assignment.AllowLateSubmit = form.AllowLateSubmit;
            assignment.MaxSubmissions = form.MaxSubmissions;
            assignment.MaxScore = form.MaxScore;

            if (form.File != null)
            {
                // Xóa file cũ nếu có
                if (!string.IsNullOrEmpty(assignment.AttachmentFilePath))
                {
                    await _fileStorage.DeleteAsync(assignment.AttachmentFilePath, ct);
                }

                // Upload file mới
                var meta = StorageObjectMetadata.ForUpload(form.File.FileName, form.File.ContentType);
                var folder = $"assignments/class-{assignment.ClassId}";
                var uploaded = await _fileStorage.SaveAsync(form.File, folder, meta, ct);

                assignment.AttachmentUrl = uploaded.url;
                assignment.AttachmentFilePath = uploaded.relativePath;
                assignment.AttachmentMimeType = form.File.ContentType;
                assignment.AttachmentSizeBytes = form.File.Length;
            }
            _assignmentRepo.Update(assignment);
            await _uow.SaveChangesAsync(ct);

            return new AssignmentDto
            {
                AssignmentId = assignment.AssignmentId,
                ClassId = assignment.ClassId,
                ScheduleId = assignment.ScheduleId,
                Title = assignment.Title,
                Description = assignment.Description,
                StartAt = assignment.StartAt,
                DueAt = assignment.DueAt,
                CloseAt = assignment.CloseAt,
                AllowLateSubmit = assignment.AllowLateSubmit,
                MaxSubmissions = assignment.MaxSubmissions,
                MaxScore = assignment.MaxScore,
                AttachmentUrl = assignment.AttachmentUrl
            };
        }


        public async Task DeleteAssignmentAsync(
            int assignmentId,
            string mentorId,
            CancellationToken ct)
        {
            var assignment = await _assignmentRepo.GetWithClassAsync(assignmentId, ct)
                ?? throw new ArgumentException("Assignment không tồn tại.");

            var canTeach = await _classRepo.IsMentorOfClassAsync(assignment.ClassId, mentorId, ct);
            if (!canTeach)
                throw new UnauthorizedAccessException("Bạn không có quyền xoá assignment này.");

            _assignmentRepo.Remove(assignment);
            await _uow.SaveChangesAsync(ct);
        }

        public async Task<List<AssignmentDto>> GetAssignmentsForClassAsync(
    int classId,
    CancellationToken ct)
        {
            var list = await _assignmentRepo.GetByClassAsync(classId, ct);

            return list.Select(a => new AssignmentDto
            {
                AssignmentId = a.AssignmentId,
                ClassId = a.ClassId,
                ScheduleId = a.ScheduleId,
                Title = a.Title,
                Description = a.Description,
                StartAt = a.StartAt,
                DueAt = a.DueAt,
                CloseAt = a.CloseAt,
                AllowLateSubmit = a.AllowLateSubmit,
                MaxSubmissions = a.MaxSubmissions,
                MaxScore = a.MaxScore,
                AttachmentUrl = a.AttachmentUrl
            }).ToList();
        }

        public async Task<List<AssignmentDto>> GetAssignmentsForStaffInClassAsync(
    int classId,
    string userId,
    CancellationToken ct)
        {
            var inClass = await _classRepo.IsInClassAsync(classId, userId, ct);
            if (!inClass)
                throw new UnauthorizedAccessException("Bạn không thuộc lớp này.");

            var list = await _assignmentRepo.GetByClassAsync(classId, ct);

            return list.Select(a => new AssignmentDto
            {
                AssignmentId = a.AssignmentId,
                ClassId = a.ClassId,
                ScheduleId = a.ScheduleId,
                Title = a.Title,
                Description = a.Description,
                StartAt = a.StartAt,
                DueAt = a.DueAt,
                CloseAt = a.CloseAt,
                AllowLateSubmit = a.AllowLateSubmit,
                MaxSubmissions = a.MaxSubmissions,
                MaxScore = a.MaxScore,
                AttachmentUrl = a.AttachmentUrl
            }).ToList();
        }

        public async Task<AssignmentDto?> GetAssignmentByIdAsync(
    int assignmentId,
    CancellationToken ct)
        {
            var a = await _assignmentRepo.GetByIdAsync(assignmentId, ct);
            if (a == null) return null;

            return new AssignmentDto
            {
                AssignmentId = a.AssignmentId,
                ClassId = a.ClassId,
                ScheduleId = a.ScheduleId,
                Title = a.Title,
                Description = a.Description,
                StartAt = a.StartAt,
                DueAt = a.DueAt,
                CloseAt = a.CloseAt,
                AllowLateSubmit = a.AllowLateSubmit,
                MaxSubmissions = a.MaxSubmissions,
                MaxScore = a.MaxScore,
                AttachmentUrl = a.AttachmentUrl
            };
        }

        public async Task<AssignmentDto?> GetAssignmentForStaffAsync(
    int assignmentId,
    string userId,
    CancellationToken ct)
        {
            var a = await _assignmentRepo.GetByIdAsync(assignmentId, ct);
            if (a == null) return null;

            var inClass = await _classRepo.IsInClassAsync(a.ClassId, userId, ct);
            if (!inClass)
                throw new UnauthorizedAccessException("Bạn không thuộc lớp này.");

            return new AssignmentDto
            {
                AssignmentId = a.AssignmentId,
                ClassId = a.ClassId,
                ScheduleId = a.ScheduleId,
                Title = a.Title,
                Description = a.Description,
                StartAt = a.StartAt,
                DueAt = a.DueAt,
                CloseAt = a.CloseAt,
                AllowLateSubmit = a.AllowLateSubmit,
                MaxSubmissions = a.MaxSubmissions,
                MaxScore = a.MaxScore,
                AttachmentUrl = a.AttachmentUrl
            };
        }

        public async Task<List<AssignmentSubmissionSummaryDto>> GetSubmissionsForAssignmentAsync(
            int assignmentId,
            string mentorId,
            CancellationToken ct)
        {
            var assignment = await _assignmentRepo.GetWithClassAsync(assignmentId, ct)
                ?? throw new ArgumentException("Assignment không tồn tại.");

            var canTeach = await _classRepo.IsMentorOfClassAsync(assignment.ClassId, mentorId, ct);
            if (!canTeach)
                throw new UnauthorizedAccessException("Bạn không có quyền xem bài nộp.");

            var list = await _submissionRepo.GetByAssignmentAsync(assignmentId, ct);
            return list.Select(s => new AssignmentSubmissionSummaryDto
            {
                SubmissionId = s.SubmissionId,
                UserId = s.UserId,
                UserFullName = s.User?.FullName ?? string.Empty,
                AttemptNumber = s.AttemptNumber,
                SubmittedAt = s.SubmittedAt,
                IsLate = s.IsLate,
                Status = s.Status,
                Score = s.Score,
                Grade = s.Grade
            }).ToList();
        }

        public async Task<List<AssignmentSubmissionSummaryDto>> GetMySubmissionsAsync(
            int assignmentId,
            string userId,
            CancellationToken ct)
        {
            var list = await _submissionRepo.GetByAssignmentAndUserAsync(assignmentId, userId, ct);
            return list.Select(s => new AssignmentSubmissionSummaryDto
            {
                SubmissionId = s.SubmissionId,
                UserId = s.UserId,
                UserFullName = s.User?.FullName ?? string.Empty,
                AttemptNumber = s.AttemptNumber,
                SubmittedAt = s.SubmittedAt,
                IsLate = s.IsLate,
                Status = s.Status,
                Score = s.Score,
                Grade = s.Grade
            }).ToList();
        }

        public async Task<AssignmentSubmissionDetailDto?> GetSubmissionDetailAsync(
            int submissionId,
            string requesterId,
            bool isMentor,
            CancellationToken ct)
        {
            var sub = await _submissionRepo.GetByIdWithFilesAndUserAsync(submissionId, ct);
            if (sub == null) return null;

            if (isMentor)
            {
                var canTeach = await _classRepo.IsMentorOfClassAsync(sub.Assignment.ClassId, requesterId, ct);
                if (!canTeach)
                    throw new UnauthorizedAccessException("Bạn không có quyền xem bài nộp này.");
            }
            else
            {
                if (sub.UserId != requesterId)
                    throw new UnauthorizedAccessException("Bạn chỉ được xem bài nộp của chính mình.");
            }

            return new AssignmentSubmissionDetailDto
            {
                SubmissionId = sub.SubmissionId,
                AssignmentId = sub.AssignmentId,
                UserId = sub.UserId,
                UserFullName = sub.User?.FullName ?? string.Empty,
                AttemptNumber = sub.AttemptNumber,
                SubmittedAt = sub.SubmittedAt,
                IsLate = sub.IsLate,
                Status = sub.Status,
                Score = sub.Score,
                Grade = sub.Grade,
                Feedback = sub.Feedback,
                Files = sub.Files.Select(f => new SubmissionFileDto
                {
                    FileId = f.FileId,
                    OriginalFileName = f.OriginalFileName,
                    FilePath = f.FilePath,
                    MimeType = f.MimeType,
                    SizeBytes = f.SizeBytes,
                    PublicUrl = f.PublicUrl,
                    IsMain = f.IsMain
                }).ToList()
            };
        }

        public async Task<AssignmentSubmissionDetailDto> CreateSubmissionAsync(
    int assignmentId,
    string userId,
    CreateSubmissionRequest request,
    (string fileName, string relativePath, string url, string? mimeType, long? sizeBytes)? file,
    CancellationToken ct)
        {
            var assignment = await _assignmentRepo.GetByIdAsync(assignmentId, ct)
                ?? throw new ArgumentException("Assignment không tồn tại.");

            var inClass = await _classRepo.IsInClassAsync(assignment.ClassId, userId, ct);
            if (!inClass)
                throw new UnauthorizedAccessException("Bạn không thuộc lớp này.");

            var now = DateTime.UtcNow;

            if (assignment.CloseAt.HasValue && now > assignment.CloseAt.Value)
                throw new InvalidOperationException("Assignment đã đóng, không thể nộp bài.");

            if (!assignment.AllowLateSubmit && assignment.DueAt.HasValue && now > assignment.DueAt.Value)
                throw new InvalidOperationException("Đã quá hạn nộp bài.");

            var maxAttempt = await _submissionRepo.GetMaxAttemptNumberAsync(assignmentId, userId, ct);
            if (maxAttempt >= assignment.MaxSubmissions)
                throw new InvalidOperationException("Bạn đã vượt quá số lần nộp cho phép.");

            var isLate = assignment.DueAt.HasValue && now > assignment.DueAt.Value;

            var submission = new AssignmentSubmission
            {
                AssignmentId = assignmentId,
                UserId = userId,
                AttemptNumber = maxAttempt + 1,
                SubmittedAt = now,
                IsLate = isLate,
                Status = AssignmentSubmissionConstants.Status.Submitted,
                Feedback = request?.Note
            };

            await _submissionRepo.AddAsync(submission, ct);
            await _uow.SaveChangesAsync(ct); // có SubmissionId

            // CHỈ 1 FILE cho mỗi submission
            if (file is not null)
            {
                var f = file.Value;
                var fileEntity = new SubmissionFile
                {
                    SubmissionId = submission.SubmissionId,
                    OriginalFileName = f.fileName,
                    FilePath = f.relativePath,
                    PublicUrl = f.url,
                    MimeType = f.mimeType,
                    SizeBytes = f.sizeBytes,
                    IsMain = true
                };

                await _fileRepo.AddRangeAsync(new[] { fileEntity }, ct);
                await _uow.SaveChangesAsync(ct);
            }

            var saved = await _submissionRepo.GetByIdWithFilesAndUserAsync(submission.SubmissionId, ct)
                ?? submission;

            return new AssignmentSubmissionDetailDto
            {
                SubmissionId = saved.SubmissionId,
                AssignmentId = saved.AssignmentId,
                UserId = saved.UserId,
                UserFullName = saved.User?.FullName ?? string.Empty,
                AttemptNumber = saved.AttemptNumber,
                SubmittedAt = saved.SubmittedAt,
                IsLate = saved.IsLate,
                Status = saved.Status,
                Score = saved.Score,
                Grade = saved.Grade,
                Feedback = saved.Feedback,
                Files = saved.Files.Select(f => new SubmissionFileDto
                {
                    FileId = f.FileId,
                    OriginalFileName = f.OriginalFileName,
                    FilePath = f.FilePath,
                    MimeType = f.MimeType,
                    SizeBytes = f.SizeBytes,
                    PublicUrl = f.PublicUrl,
                    IsMain = f.IsMain
                }).ToList()
            };
        }


        public async Task<AssignmentSubmissionDetailDto> GradeSubmissionAsync(
            int submissionId,
            string mentorId,
            GradeSubmissionDto dto,
            CancellationToken ct)
        {
            var sub = await _submissionRepo.GetByIdWithFilesAndUserAsync(submissionId, ct)
                ?? throw new ArgumentException("Submission không tồn tại.");

            var assignment = sub.Assignment ?? throw new InvalidOperationException("Assignment not loaded.");
            var canTeach = await _classRepo.IsMentorOfClassAsync(assignment.ClassId, mentorId, ct);
            if (!canTeach)
                throw new UnauthorizedAccessException("Bạn không có quyền chấm bài này.");

            if (dto.Score.HasValue && assignment.MaxScore.HasValue &&
                (dto.Score < 0 || dto.Score > assignment.MaxScore.Value))
            {
                throw new InvalidOperationException("Điểm không hợp lệ.");
            }

            sub.Score = dto.Score;
            sub.Grade = dto.Grade;
            sub.Feedback = dto.Feedback ?? sub.Feedback;
            if (!string.IsNullOrEmpty(dto.Status))
                sub.Status = dto.Status;
            else
                sub.Status = AssignmentSubmissionConstants.Status.Graded;

            _submissionRepo.Update(sub);
            await _uow.SaveChangesAsync(ct);

            return new AssignmentSubmissionDetailDto
            {
                SubmissionId = sub.SubmissionId,
                AssignmentId = sub.AssignmentId,
                UserId = sub.UserId,
                UserFullName = sub.User?.FullName ?? string.Empty,
                AttemptNumber = sub.AttemptNumber,
                SubmittedAt = sub.SubmittedAt,
                IsLate = sub.IsLate,
                Status = sub.Status,
                Score = sub.Score,
                Grade = sub.Grade,
                Feedback = sub.Feedback,
                Files = sub.Files.Select(f => new SubmissionFileDto
                {
                    FileId = f.FileId,
                    OriginalFileName = f.OriginalFileName,
                    FilePath = f.FilePath,
                    MimeType = f.MimeType,
                    SizeBytes = f.SizeBytes,
                    PublicUrl = f.PublicUrl,
                    IsMain = f.IsMain
                }).ToList()
            };
        }

    }
}
