namespace InternalTrainingSystem.Core.DTOs
{
    public class CourseStatisticsDto
    {
        public class TrainingOverviewByMonthFilterDto
        {
            public int Year { get; set; }
            public int? Month { get; set; }      
        }

        public class TrainingOverviewStatsDto
        {
            public int Year { get; set; }
            public int? Month { get; set; }

            public int TotalCoursesOpened { get; set; }
            public int TotalEmployeesTrained { get; set; }
        }

        public class TrainingOverviewClassStatsDto
        {
            public int Year { get; set; }
            public int? Month { get; set; }

            public int TotalClassOpened { get; set; }
            public int TotalEmployeesTrained { get; set; }
            public List<int> ClassIds { get; set; } = new();
        }

    }
}
