using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace LMSProject.ViewModels
{
    public class UploadAssignmentViewModel
    {
        [Required]
        public string Title { get; set; } = string.Empty;

        [Required]
        public string Description { get; set; } = string.Empty;

        [Required]
        public DateTime DueDate { get; set; }

        [Required]
        public int CourseId { get; set; }

        [Required]
        public List<string> SelectedClasses { get; set; } = new();

        [Required]
        public IFormFile File { get; set; }
    }
}
