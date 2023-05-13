namespace TaskPlanner.Models;

public class IndexViewModel
{
    public ICollection<Project> Projects { get; set; }
    
    public ICollection<MTask> MTasksHighPreority { get; set; }
}