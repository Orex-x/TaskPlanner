using Microsoft.AspNetCore.Mvc;

namespace TaskPlanner.Models;

public class CreateMTaskViewModel
{
    public MTask MTask { get; set; }
    
    public int idProject { get; set; }
    public bool IsAdmin { get; set; }
    
    public Dictionary<string, Boolean> SelectedUsers { get; set; }
    
    [BindProperty]
    public BufferedSingleMultiplyFileUploadDb FileUpload { get; set; }
}