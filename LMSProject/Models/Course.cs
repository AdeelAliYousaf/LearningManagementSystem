using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace LMSProject.Models
{
    public class Course
    {
        public int Id { get; set; }

        [Required]
        public required string Title { get; set; }

        [Required]
        public required string Description { get; set; }

        [Required]
        public string Department { get; set; }
        public string? TeacherId { get; set; }

        [ForeignKey("TeacherId")]
        public ApplicationUser? Teacher { get; set; }

        public ICollection<Enrollment> Enrollments { get; set; } = new List<Enrollment>();
        public ICollection<Assignment> Assignments { get; set; } = new List<Assignment>();

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}