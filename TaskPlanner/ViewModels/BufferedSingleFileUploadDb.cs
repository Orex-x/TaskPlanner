using System.ComponentModel.DataAnnotations;

namespace TaskPlanner.Models;

public class BufferedSingleFileUploadDb
{
    [Required]
    [Display(Name="File")]
    public IFormFile FormFile { get; set; }
}