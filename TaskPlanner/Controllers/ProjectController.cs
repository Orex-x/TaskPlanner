using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TaskPlanner.Models;
using TaskPlanner.Services;

namespace TaskPlanner.Controllers;

public class ProjectController : Controller
{
    private readonly DatabaseContext _context;

    public ProjectController(DatabaseContext context)
    {
        _context = context;
    }

    [Authorize]
    public async Task<IActionResult> MainProjectPage(int idProject, int idGroup, bool isAdmin)
    {

        var email = User.Identity?.Name;
        var user = _context.Users
            .Include(x => x.FavoriteMTasks)
            .FirstOrDefault(x => x.Email == email);
        
        var project = await _context.Projects
            .Include(x => x.MTasks)
            .FirstOrDefaultAsync(x => x.Id == idProject);
        
        var model = new MainProjectPageViewModel
        {
            Project = project!,
            IsAdmin = isAdmin,
            IdGroup = idGroup,
            User = user,
            
        };
        return View(model);
    }
    
    [Authorize]
    public IActionResult CreateProject(int idGroup, int idUserGroup)
    {
        var model = new CreateProjectViewModel
        {
            IdGroup = idGroup,
            IdUserGroup = idUserGroup,
            Project = new Project()
        };

        return View(model);
    }
    
    [Authorize]
    [HttpPost]
    public async Task<IActionResult> CreateProject(CreateProjectViewModel model)
    {
        if (!ModelState.IsValid) return View(model);

        var group = await _context.Groups
            .Include(x => x.Projects)
            .FirstOrDefaultAsync(x => x.Id == model.IdGroup);

        group!.Projects.Add(model.Project);

        _context.Groups.Update(group);
        await _context.SaveChangesAsync();
        

        return RedirectToAction("GroupPage", "Group", new { id = model.IdUserGroup });
    }
    
    [Authorize]
    public async Task<IActionResult> RemoveProject(int idProject, int idUserGroup)
    {
        var project = await _context.Projects
            .Include(x => x.MTasks)
            .FirstOrDefaultAsync(x => x.Id == idProject);

        if (project != null)
        {
            var listMTasks = project.MTasks.ToList();
        
            listMTasks.ForEach(async task =>
            {
                var list = _context.MTasks.Where(x => x.Code == task.Code);
                await list.ForEachAsync(x => _context.MTasks.Remove(x));
            });
            
            _context.Projects.Remove(project);
            await _context.SaveChangesAsync();    
        }
        
        return RedirectToAction("GroupPage", "Group", new { id = idUserGroup });
    }
    
}
    
