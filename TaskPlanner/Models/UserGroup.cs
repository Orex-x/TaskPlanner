namespace TaskPlanner.Models;

public class UserGroup
{
    public int Id { get; set; }
    
    public Group Group { get; set; }
    
    public User User { get; set; }
    
    public bool IsAdmin { get; set; }
}