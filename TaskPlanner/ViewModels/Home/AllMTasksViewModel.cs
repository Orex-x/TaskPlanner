namespace TaskPlanner.Models;

public class AllMTasksViewModel
{
    public ICollection<MTask> MTasks { get; set; }
    
    public User User { get; set; }
}