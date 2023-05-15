using Microsoft.AspNetCore.Mvc;

namespace TaskPlanner.Models;

public class CreateGroupViewModel
{
    public Group Group { get; set; }
    
    [BindProperty]
    public BufferedSingleFileUploadDb FileUpload { get; set; }
}