namespace LMSProject.Models
{
    public class Assignment
    {
        public int Id { get; set; }

        public required string Title { get; set; }

        public required string Description { get; set; }

        public DateTime DueDate { get; set; }

        public int CourseId { get; set; }

        public Course? Course { get; set; }

        public string FilePath { get; set; } = string.Empty;

        // NEW: store assigned class names as comma-separated string
        public string TargetClasses { get; set; } = string.Empty;

        public required ICollection<Submission> Submissions { get; set; }
    }

}
