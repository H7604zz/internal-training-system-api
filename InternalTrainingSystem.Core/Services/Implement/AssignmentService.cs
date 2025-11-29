using Humanizer;
using InternalTrainingSystem.Core.Common.Constants;
using InternalTrainingSystem.Core.DTOs;
using InternalTrainingSystem.Core.Helper;
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
        private readonly IClassRepository _classRepo;
        private readonly IFileStorage _fileStorage;
        private readonly IUnitOfWork _uow;

        public AssignmentService(
            IAssignmentRepository assignmentRepo,
            IAssignmentSubmissionRepository submissionRepo,
            IClassRepository classRepo,
            IFileStorage fileStorage,
            IUnitOfWork uow)
        {
            _assignmentRepo = assignmentRepo;
            _submissionRepo = submissionRepo;
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

            var exists = await _assignmentRepo.ExistsInClassAsync(form.ClassId, ct);
            if (exists)
                throw new InvalidOperationException("Lớp này đã có assignment. Không thể tạo thêm.");
            var classInfo = await _classRepo.GetClassDetailAsync(form.ClassId);
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
                size = form.File.Length;
            }

            var entity = new Assignment
            {
                ClassId = form.ClassId,
                ScheduleId = form.ScheduleId,
                Title = form.Title,
                Description = form.Description,
                StartAt = DateTimeUtils.Now(),
                DueAt = form.DueAt,

                AttachmentUrl = url,
                AttachmentFilePath = path,
                AttachmentMimeType = mime,
                AttachmentSizeBytes = size
            };

            await _assignmentRepo.AddAsync(entity, ct);
            await _uow.SaveChangesAsync(ct);

            //gửi mail
            var staffs = await _classRepo.GetUsersInClassAsync(form.ClassId);

            string subject = $"Bài cuối kì mới – {entity.Title}";

            string html = $@"
<div style='font-family: Arial; max-width: 600px; margin:auto;'>
    <h2 style='color:#0d6efd;'>Thông báo bài tập mới</h2>
    <p>Bạn có bài tập mới trong lớp <strong>{classInfo.ClassName}</strong>.</p>

    <div style='padding: 15px; background: #f8f9fa; border-radius: 5px; margin:10px 0'>
        <p><strong>Tiêu đề:</strong> {entity.Title}</p>
        <p><strong>Mô tả:</strong> {entity.Description}</p>
        <p><strong>Ngày bắt đầu:</strong> {entity.StartAt:dd/MM/yyyy HH:mm}</p>
        <p><strong>Hạn nộp:</strong> {entity.DueAt:dd/MM/yyyy HH:mm}</p>
    </div>

    <p>Truy cập hệ thống để xem chi tiết.</p>
</div>";


            foreach (var s in staffs.Users)
            {
                if (!string.IsNullOrEmpty(s.Email))
                {
                    Hangfire.BackgroundJob.Enqueue(() => EmailHelper.SendEmailAsync(
                        s.Email!,
                        subject,
                        html
                    ));
                }
            }

            return new AssignmentDto
            {
                AssignmentId = entity.AssignmentId,
                ClassId = entity.ClassId,
                ScheduleId = entity.ScheduleId,
                Title = entity.Title,
                Description = entity.Description,
                StartAt = entity.StartAt,
                DueAt = entity.DueAt,
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
            assignment.DueAt = form.DueAt;

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

            var oldFile = assignment.AttachmentFilePath;

            _assignmentRepo.Remove(assignment);
            await _uow.SaveChangesAsync(ct);

            if (!string.IsNullOrEmpty(oldFile))
            {
                await _fileStorage.DeleteAsync(oldFile, ct);
            }
        }


        public async Task<AssignmentDto?> GetAssignmentForClassAsync(
    int classId,
    string mentorId,
    CancellationToken ct)
        {
            var canTeach = await _classRepo.IsMentorOfClassAsync(classId, mentorId, ct);
            if (!canTeach)
                throw new UnauthorizedAccessException("Bạn không phải Mentor của lớp này.");

            var a = await _assignmentRepo.GetSingleByClassAsync(classId, ct);
            if (a == null) return null;

            var submissions = await _submissionRepo.GetByAssignmentAsync(a.AssignmentId, ct);

            return new AssignmentDto
            {
                AssignmentId = a.AssignmentId,
                ClassId = a.ClassId,
                ScheduleId = a.ScheduleId,
                Title = a.Title,
                Description = a.Description,
                StartAt = a.StartAt,
                DueAt = a.DueAt,
                AttachmentUrl = a.AttachmentUrl,

                Submissions = submissions.Select(s => new AssignmentSubmissionSummaryDto
                {
                    SubmissionId = s.SubmissionId,
                    UserId = s.UserId,
                    FullName = s.User.FullName,
                    EmployeeId = s.User?.EmployeeId ?? "N/A",
                    SubmittedAt = s.SubmittedAt,
                    PublicUrl = s.PublicUrl,
                }).ToList()
            };
        }

        public async Task<AssignmentDto?> GetAssignmentForStaffInClassAsync(
    int classId,
    string userId,
    CancellationToken ct)
        {
            var inClass = await _classRepo.IsInClassAsync(classId, userId, ct);
            if (!inClass)
                throw new UnauthorizedAccessException("Bạn không thuộc lớp này.");

            var a = await _assignmentRepo.GetSingleByClassAsync(classId, ct);
            if (a == null) return null;
            var submission = await _submissionRepo.GetByAssignmentAndUserSingleAsync(
        a.AssignmentId,
        userId,
        ct
    );

            var dto = new AssignmentDto
            {
                AssignmentId = a.AssignmentId,
                ClassId = a.ClassId,
                ScheduleId = a.ScheduleId,
                Title = a.Title,
                Description = a.Description,
                StartAt = a.StartAt,
                DueAt = a.DueAt,
                AttachmentUrl = a.AttachmentUrl,
                Submissions = new List<AssignmentSubmissionSummaryDto>()
            };

            if (submission != null)
            {
                dto.Submissions.Add(new AssignmentSubmissionSummaryDto
                {
                    SubmissionId = submission.SubmissionId,
                    UserId = submission.UserId,
                    FullName = submission.User?.FullName ?? "",
                    EmployeeId = submission.User?.EmployeeId ?? "N/A",
                    SubmittedAt = submission.SubmittedAt,
                    PublicUrl = submission.PublicUrl
                });
            }

            return dto;
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
                FullName = s.User?.FullName ?? string.Empty,
                SubmittedAt = s.SubmittedAt,
                PublicUrl = s.PublicUrl,
            }).ToList();
        }

        public async Task<AssignmentSubmissionDetailDto?> GetSubmissionDetailAsync(int submissionId,
                                                                                   string requesterId,
                                                                                   bool isMentor,
                                                                                   CancellationToken ct)
        {
            var sub = await _submissionRepo.GetByIdWithUserAsync(submissionId, ct);
            if (sub == null) return null;

            if (isMentor)
            {
                var canTeach = await _classRepo.IsMentorOfClassAsync(sub.Assignment.ClassId, requesterId, ct);
                if (!canTeach)
                    throw new UnauthorizedAccessException("Bạn không phải Mentor của lớp này.");
            }
            else if (sub.UserId != requesterId)
                throw new UnauthorizedAccessException("Không được xem bài nộp của người khác.");

            return new AssignmentSubmissionDetailDto
            {
                SubmissionId = sub.SubmissionId,
                AssignmentId = sub.AssignmentId,
                UserId = sub.UserId,
                UserFullName = sub.User.FullName,
                SubmittedAt = sub.SubmittedAt,

                FilePath = sub.FilePath,
                MimeType = sub.MimeType,
                SizeBytes = sub.SizeBytes,
                PublicUrl = sub.PublicUrl
            };
        }

        public async Task<AssignmentSubmissionDetailDto> CreateSubmissionAsync(
    int assignmentId,
    string userId,
    (string fileName, string relativePath, string url, string? mimeType, long? sizeBytes)? file,
    CancellationToken ct)
        {
            var assignment = await _assignmentRepo.GetByIdAsync(assignmentId, ct)
                ?? throw new ArgumentException("Assignment không tồn tại.");

            var inClass = await _classRepo.IsInClassAsync(assignment.ClassId, userId, ct);
            if (!inClass)
                throw new UnauthorizedAccessException("Bạn không thuộc lớp này.");

            var now = DateTimeUtils.Now();

            if (assignment.DueAt.HasValue && now > assignment.DueAt.Value)
                throw new InvalidOperationException("Đã quá hạn nộp bài. Bạn không thể nộp nữa.");

            var existing = await _submissionRepo.GetByAssignmentAndUserSingleAsync(assignmentId, userId, ct);

            AssignmentSubmission submission;

            if (existing != null)
            {
                // update file
                submission = existing;
                submission.SubmittedAt = now;

                if (file is not null)
                {
                    if (!string.IsNullOrEmpty(existing.FilePath))
                        await _fileStorage.DeleteAsync(existing.FilePath, ct);

                    var f = file.Value;
                    submission.FilePath = f.relativePath;
                    submission.PublicUrl = f.url;
                    submission.MimeType = f.mimeType;
                    submission.SizeBytes = f.sizeBytes;
                }

                _submissionRepo.Update(submission);
            }
            else
            {
                submission = new AssignmentSubmission
                {
                    AssignmentId = assignmentId,
                    UserId = userId,
                    SubmittedAt = now,
                };

                if (file is not null)
                {
                    var f = file.Value;
                    submission.FilePath = f.relativePath;
                    submission.PublicUrl = f.url;
                    submission.MimeType = f.mimeType;
                    submission.SizeBytes = f.sizeBytes;
                }

                await _submissionRepo.AddAsync(submission, ct);
            }

            await _uow.SaveChangesAsync(ct);

            var saved = await _submissionRepo.GetByIdWithUserAsync(submission.SubmissionId, ct);

            return new AssignmentSubmissionDetailDto
            {
                SubmissionId = saved!.SubmissionId,
                AssignmentId = saved.AssignmentId,
                UserId = saved.UserId,
                UserFullName = saved.User.FullName,
                SubmittedAt = saved.SubmittedAt,
                FilePath = saved.FilePath,
                MimeType = saved.MimeType,
                SizeBytes = saved.SizeBytes,
                PublicUrl = saved.PublicUrl
            };
        }
    }
}
