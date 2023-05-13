namespace TaskPlanner.Models;

public class User
{
    public int Id { get; set; }
    
    public string FirstName { get; set; }
    public string SecondName { get; set; }
    public string LastName { get; set; }
    public string Login { get; set; }
    public string Password { get; set; }
    public string Email { get; set; }
    public string Phone { get; set; }
    public string PathToAvatar { get; set; }
    
    public virtual ICollection<MTask> MTasks { get; set; } = new List<MTask>();
}