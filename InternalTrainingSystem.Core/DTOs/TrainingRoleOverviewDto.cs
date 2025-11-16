namespace InternalTrainingSystem.Core.DTOs
{
    public class TrainingRoleOverviewDto
    {
        public int Year { get; set; }
        public int Month { get; set; }

        // Bao nhiêu course được mở / NV được đào tạo (theo course)
        public int TotalCoursesOpened { get; set; }
        public int TotalEmployeesTrainedByCourse { get; set; }

        // Bao nhiêu lớp được mở / NV được đào tạo (theo class)
        public int TotalClassesOpened { get; set; }
        public int TotalStudentsInClasses { get; set; }

        // Thống kê tỉ lệ pass / bao nhiêu lớp
        public int TotalClassesHaveStats { get; set; }      // số lớp có dữ liệu tính pass
        public int TotalPassedStudents { get; set; }        // tổng số học viên pass
        public double AverageClassPassRate { get; set; }    // trung bình % pass của các lớp
    }

}
