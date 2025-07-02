using Microsoft.AspNetCore.Http;
using System.ComponentModel.DataAnnotations;

namespace LMSProject.Models.ViewModels
{
    public class SubmitAssignmentViewModel
    {
        [Required]
        public int AssignmentId { get; set; }

        [Required(ErrorMessage = "Please upload your assignment file.")]
        [DataType(DataType.Upload)]
        [Display(Name = "Assignment File")]
        public IFormFile File { get; set; } = null!;
    }
}
