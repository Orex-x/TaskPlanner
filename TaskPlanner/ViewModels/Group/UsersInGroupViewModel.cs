namespace TaskPlanner.Models;

public class UsersInGroupViewModel
{
    public ICollection<UserGroup> UserGroups { get; set; }
    
    public bool IsAdmin { get; set; }
    
    public string InviteLink { get; set; }
}