namespace TaskPlanner.Models;

public class MainMTaskPageViewModel
{
    public MTask MTask { get; set; }
    
    public User Author { get; set; }
    
    public bool isAuthor { get; set; }
}