using System.ComponentModel.DataAnnotations;

namespace TaskPlanner.Models;

public class BufferedSingleMultiplyFileUploadDb
{
    [Required]
    [Display(Name="File")]
    public List<IFormFile> FormFiles { get; set; }
}