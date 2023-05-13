namespace TaskPlanner.Models;

public class MainGroupPageViewModel
{
    public ICollection<UserGroup> UserGroups { get; set; }
    
    public User User { get; set; }
    
    public string Link { get; set; }
}