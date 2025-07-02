
namespace LMSProject.Models
{
    public class Announcement
    {
        public int Id { get; set; }

        public required string Title { get; set; }

        public required string Message { get; set; }

        public string? StudentId { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public int CourseId { get; set; }
        public Course? Course { get; set; }

        public string? PostedById { get; set; }
        public ApplicationUser? PostedBy { get; set; }
    }
}
