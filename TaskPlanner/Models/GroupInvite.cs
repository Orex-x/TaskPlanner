namespace TaskPlanner.Models;

public class GroupInvite
{
    public int Id { get; set; }
    
    public User InvitedUser { get; set; }
    public User InvitingUser { get; set; }
    public Group Group { get; set; }
    public string Title { get; set; }
}