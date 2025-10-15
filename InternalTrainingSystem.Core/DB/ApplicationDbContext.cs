using InternalTrainingSystem.Core.Models;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using System.Reflection.Emit;

namespace InternalTrainingSystem.Core.DB
{
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public DbSet<CourseCategory> CourseCategories { get; set; }
        public DbSet<Course> Courses { get; set; }
        public DbSet<CourseEnrollment> CourseEnrollments { get; set; }
        public DbSet<Quiz> Quizzes { get; set; }
        public DbSet<Question> Questions { get; set; }
        public DbSet<Answer> Answers { get; set; }
        public DbSet<QuizAttempt> QuizAttempts { get; set; }
        public DbSet<UserAnswer> UserAnswers { get; set; }
        public DbSet<Schedule> Schedules { get; set; }
        public DbSet<ScheduleParticipant> ScheduleParticipants { get; set; }
        public DbSet<CourseHistory> CourseHistories { get; set; }
        public DbSet<Attendance> Attendances { get; set; }
        public DbSet<UserRoleHistory> UserRoleHistories { get; set; }
        public DbSet<Certificate> Certificates { get; set; }
        public DbSet<Class> Classes { get; set; }
        public DbSet<ClassEnrollment> ClassEnrollments { get; set; }
        public DbSet<CourseNotification> CourseNotifications { get; set; }
        public DbSet<CourseModule> CourseModules { get; set; }
        public DbSet<Lesson> Lessons { get; set; }
        public DbSet<Department> Departments { get; set; }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            // Course relationships
            builder.Entity<Course>()
                .HasOne(c => c.CreatedBy)
                .WithMany(u => u.CreatedCourses)
                .HasForeignKey(c => c.CreatedById)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<Course>()
                .HasOne(c => c.CourseCategory)
                .WithMany(cat => cat.Courses)
                .HasForeignKey(c => c.CourseCategoryId)
                .OnDelete(DeleteBehavior.Restrict);

            // CourseEnrollment relationships
            builder.Entity<CourseEnrollment>()
                .HasOne(ce => ce.User)
                .WithMany(u => u.CourseEnrollments)
                .HasForeignKey(ce => ce.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.Entity<CourseEnrollment>()
                .HasOne(ce => ce.Course)
                .WithMany(c => c.CourseEnrollments)
                .HasForeignKey(ce => ce.CourseId)
                .OnDelete(DeleteBehavior.Cascade);

            // Quiz relationships
            builder.Entity<Quiz>()
                .HasOne(q => q.Course)
                .WithMany(c => c.Quizzes)
                .HasForeignKey(q => q.CourseId)
                .OnDelete(DeleteBehavior.Cascade);

            // Question relationships
            builder.Entity<Question>()
                .HasOne(q => q.Quiz)
                .WithMany(qz => qz.Questions)
                .HasForeignKey(q => q.QuizId)
                .OnDelete(DeleteBehavior.Cascade);

            // Answer relationships
            builder.Entity<Answer>()
                .HasOne(a => a.Question)
                .WithMany(q => q.Answers)
                .HasForeignKey(a => a.QuestionId)
                .OnDelete(DeleteBehavior.Cascade);

            // QuizAttempt relationships
            builder.Entity<QuizAttempt>()
                .HasOne(qa => qa.User)
                .WithMany(u => u.QuizAttempts)
                .HasForeignKey(qa => qa.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.Entity<QuizAttempt>()
                .HasOne(qa => qa.Quiz)
                .WithMany(q => q.QuizAttempts)
                .HasForeignKey(qa => qa.QuizId)
                .OnDelete(DeleteBehavior.Cascade);

            // UserAnswer relationships
            builder.Entity<UserAnswer>()
                .HasOne(ua => ua.QuizAttempt)
                .WithMany(qa => qa.UserAnswers)
                .HasForeignKey(ua => ua.AttemptId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.Entity<UserAnswer>()
                .HasOne(ua => ua.Question)
                .WithMany(q => q.UserAnswers)
                .HasForeignKey(ua => ua.QuestionId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<UserAnswer>()
                .HasOne(ua => ua.Answer)
                .WithMany(a => a.UserAnswers)
                .HasForeignKey(ua => ua.AnswerId)
                .OnDelete(DeleteBehavior.Restrict);

            // Schedule relationships
            builder.Entity<Schedule>()
                .HasOne(s => s.Course)
                .WithMany(c => c.Schedules)
                .HasForeignKey(s => s.CourseId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.Entity<Schedule>()
                .HasOne(s => s.Instructor)
                .WithMany(u => u.Schedules)
                .HasForeignKey(s => s.InstructorId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<Schedule>()
                .HasOne(s => s.Class)
                .WithMany(cl => cl.Schedules)
                .HasForeignKey(s => s.ClassId)
                .OnDelete(DeleteBehavior.SetNull);

            // ScheduleParticipant relationships
            builder.Entity<ScheduleParticipant>()
                .HasOne(sp => sp.Schedule)
                .WithMany(s => s.ScheduleParticipants)
                .HasForeignKey(sp => sp.ScheduleId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.Entity<ScheduleParticipant>()
                .HasOne(sp => sp.User)
                .WithMany()
                .HasForeignKey(sp => sp.UserId)
                .OnDelete(DeleteBehavior.Restrict);

            // Attendance relationships and composite key
            builder.Entity<Attendance>()
                .HasKey(a => new { a.UserId, a.ScheduleId });

            builder.Entity<Attendance>()
                .HasOne(a => a.User)
                .WithMany(u => u.Attendances)
                .HasForeignKey(a => a.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.Entity<Attendance>()
                .HasOne(a => a.Schedule)
                .WithMany(s => s.Attendances)
                .HasForeignKey(a => a.ScheduleId)
                .OnDelete(DeleteBehavior.Cascade);

            // Indexes for better performance
            builder.Entity<CourseEnrollment>()
                .HasIndex(ce => new { ce.UserId, ce.CourseId })
                .IsUnique();

            builder.Entity<QuizAttempt>()
                .HasIndex(qa => new { qa.UserId, qa.QuizId, qa.AttemptNumber });

            builder.Entity<ScheduleParticipant>()
                .HasIndex(sp => new { sp.ScheduleId, sp.UserId })
                .IsUnique();

            // Indexes for Attendance
            builder.Entity<Attendance>()
                .HasIndex(a => a.CheckInTime);

            builder.Entity<Attendance>()
                .HasIndex(a => a.Status);

            // CourseHistory relationships
            builder.Entity<CourseHistory>()
                .HasOne(ch => ch.User)
                .WithMany(u => u.CourseHistories)
                .HasForeignKey(ch => ch.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.Entity<CourseHistory>()
                .HasOne(ch => ch.Course)
                .WithMany(c => c.CourseHistories)
                .HasForeignKey(ch => ch.CourseId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.Entity<CourseHistory>()
                .HasOne(ch => ch.Enrollment)
                .WithMany(ce => ce.CourseHistories)
                .HasForeignKey(ch => ch.EnrollmentId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<CourseHistory>()
                .HasOne(ch => ch.QuizAttempt)
                .WithMany(qa => qa.CourseHistories)
                .HasForeignKey(ch => ch.QuizAttemptId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<CourseHistory>()
                .HasOne(ch => ch.Schedule)
                .WithMany(s => s.CourseHistories)
                .HasForeignKey(ch => ch.ScheduleId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<Certificate>()
                .HasOne(c => c.Course)
                .WithMany(course => course.Certificates)
                .HasForeignKey(c => c.CourseId);

            builder.Entity<Certificate>()
                .HasOne(c => c.User)
                .WithMany(u => u.Certificates)
                .HasForeignKey(c => c.UserId);

            // Indexes for CourseCategory
            builder.Entity<CourseCategory>()
                .HasIndex(c => c.CategoryName)
                .IsUnique();

            // Indexes for CourseHistory
            builder.Entity<CourseHistory>()
                .HasIndex(ch => new { ch.UserId, ch.CourseId, ch.ActionDate });

            builder.Entity<CourseHistory>()
                .HasIndex(ch => ch.Action);

            builder.Entity<CourseHistory>()
                .HasIndex(ch => ch.ActionDate);

            // Precision for decimal properties
            builder.Entity<QuizAttempt>()
                .Property(qa => qa.Percentage)
                .HasPrecision(5, 2);

            // UserRoleHistory unique constraint for business logic
            builder.Entity<UserRoleHistory>()
                .HasIndex(urh => new { urh.UserId, urh.RoleId, urh.ActionDate })
                .IsUnique();

            // UserRoleHistory relationships
            builder.Entity<UserRoleHistory>()
                .HasOne(urh => urh.User)
                .WithMany()
                .HasForeignKey(urh => urh.UserId)
                .OnDelete(DeleteBehavior.Restrict); // Thay đổi từ Cascade thành Restrict

            builder.Entity<UserRoleHistory>()
                .HasOne(urh => urh.ActionByUser)
                .WithMany()
                .HasForeignKey(urh => urh.ActionBy)
                .OnDelete(DeleteBehavior.SetNull);

            // Indexes for UserRoleHistory
            builder.Entity<UserRoleHistory>()
                .HasIndex(urh => new { urh.UserId, urh.ActionDate });

            builder.Entity<UserRoleHistory>()
                .HasIndex(urh => urh.Action);

            builder.Entity<UserRoleHistory>()
                .HasIndex(urh => urh.ActionDate);

            builder.Entity<UserRoleHistory>()
                .HasIndex(urh => urh.RoleId);

            // Indexes for CourseCategory
            builder.Entity<Certificate>()
                .HasIndex(c => new { c.UserId, c.CourseId })
                .IsUnique();

            // Class relationships
            builder.Entity<Class>()
                .HasOne(cl => cl.Course)
                .WithMany(c => c.Classes)
                .HasForeignKey(cl => cl.CourseId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<Class>()
                .HasOne(cl => cl.Mentor)
                .WithMany(u => u.MentoredClasses)
                .HasForeignKey(cl => cl.MentorId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<Class>()
                .HasOne(cl => cl.CreatedBy)
                .WithMany(u => u.CreatedClasses)
                .HasForeignKey(cl => cl.CreatedById)
                .OnDelete(DeleteBehavior.Restrict);

            // ClassEnrollment relationships
            builder.Entity<ClassEnrollment>()
                .HasOne(ce => ce.Class)
                .WithMany(cl => cl.ClassEnrollments)
                .HasForeignKey(ce => ce.ClassId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.Entity<ClassEnrollment>()
                .HasOne(ce => ce.Student)
                .WithMany(u => u.ClassEnrollments)
                .HasForeignKey(ce => ce.StudentId)
                .OnDelete(DeleteBehavior.Restrict);

            // Indexes for Class
            builder.Entity<Class>()
                .HasIndex(cl => cl.ClassName);

            builder.Entity<Class>()
                .HasIndex(cl => new { cl.CourseId, cl.MentorId });

            builder.Entity<Class>()
                .HasIndex(cl => cl.Status);

            builder.Entity<Class>()
                .HasIndex(cl => new { cl.StartDate, cl.EndDate });

            // Indexes for ClassEnrollment
            builder.Entity<ClassEnrollment>()
                .HasIndex(ce => new { ce.ClassId, ce.StudentId })
                .IsUnique();

            builder.Entity<ClassEnrollment>()
                .HasIndex(ce => ce.Status);

            builder.Entity<ClassEnrollment>()
                .HasIndex(ce => ce.EnrollmentDate);

            // Precision for decimal properties in ClassEnrollment
            builder.Entity<ClassEnrollment>()
                .Property(ce => ce.FinalGrade)
                .HasPrecision(5, 2);

            builder.Entity<CourseNotification>()
                .HasIndex(c => new { c.CourseId, c.Type })
                .IsUnique();
            // Department relationships
            builder.Entity<Department>(e =>
            {
                e.Property(d => d.Name).IsRequired().HasMaxLength(200);
                e.Property(d => d.Description).HasMaxLength(1000);

                e.HasIndex(d => d.Name).IsUnique();

                e.HasOne(d => d.User)
                 .WithOne(u => u.Department)
                 .HasForeignKey<Department>(d => d.UserId)
                 .OnDelete(DeleteBehavior.SetNull);
            });
            
            builder.Entity<Department>()
                   .HasMany(d => d.Courses)
                   .WithMany(c => c.Departments)
                   .UsingEntity<Dictionary<string, object>>(
                       "DepartmentCourse",
                       r => r.HasOne<Course>()
                             .WithMany()
                             .HasForeignKey("CourseId")
                             .OnDelete(DeleteBehavior.Cascade),
                       l => l.HasOne<Department>()
                             .WithMany()
                             .HasForeignKey("DepartmentId")
                             .OnDelete(DeleteBehavior.Cascade),
                       j =>
                       {
                           j.HasKey("DepartmentId", "CourseId");
                           j.HasIndex("CourseId", "DepartmentId"); 
                       }
                   );

            // CourseModule
            builder.Entity<CourseModule>()
                .HasOne(m => m.Course)
                .WithMany(c => c.Modules)                    
                .HasForeignKey(m => m.CourseId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.Entity<CourseModule>()
                .HasIndex(m => new { m.CourseId, m.OrderIndex }); 

            // Lesson
            builder.Entity<Lesson>()
                .HasOne(l => l.Module)
                .WithMany(m => m.Lessons)
                .HasForeignKey(l => l.ModuleId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.Entity<Lesson>()
                .HasIndex(l => new { l.ModuleId, l.OrderIndex }); 

            
            builder.Entity<Lesson>()
                .HasIndex(l => l.Type);
        }
    }
}