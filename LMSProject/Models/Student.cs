using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace LMSProject.Models
{
    public class Student
    {
        public int Id { get; set; }

        [Required]
        public string UserId { get; set; }

        [ForeignKey("UserId")]
        public ApplicationUser? User { get; set; } // ✅ make nullable

        [Required]
        public string RollNumber { get; set; }

        [Required]
        public string RegistrationNumber { get; set; }

        [Required]
        public string Class { get; set; }
    }
}