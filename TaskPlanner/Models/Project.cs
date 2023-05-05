namespace TaskPlanner.Models;

public class Project
{
    public int Id { get; set; }
    
    public string Name { get; set; }
    
    public string Description { get; set; }
    
    public virtual ICollection<MTask> MTasks { get; set; } = new List<MTask>();
}