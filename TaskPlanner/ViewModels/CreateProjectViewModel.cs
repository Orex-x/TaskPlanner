namespace TaskPlanner.Models;

public class CreateProjectViewModel
{
    public Project Project { get; set; }
    
    public int IdGroup { get; set; }
    public int IdUserGroup { get; set; }
}