namespace TaskPlanner.Models;

public class MainProjectPageViewModel
{
    public Project Project { get; set; }
    public bool IsAdmin { get; set; }
    
    public int IdGroup { get; set; }
    
    public int SortRule { get; set; }
    
}