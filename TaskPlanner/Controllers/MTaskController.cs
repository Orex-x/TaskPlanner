using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TaskPlanner.Models;
using TaskPlanner.Services;

namespace TaskPlanner.Controllers;

public class MTaskController : Controller
{
    private readonly DatabaseContext _context;

    public MTaskController(DatabaseContext context)
    {
        _context = context;
    }

 
    public async Task<IActionResult> CreateMTask(int idProject, int idGroup, bool isAdmin)
    {

        var users = _context.UserGroups
            .Where(x => x.Group.Id == idGroup)
            .Select(x => x.User);

        var selectedUsers = new Dictionary<string, Boolean>();
        
        await users.ForEachAsync(user => selectedUsers.Add(user.Email, true));

        var model = new CreateMTaskViewModel
        {
            idProject = idProject,
            MTask = new MTask(),
            IsAdmin = isAdmin,
            SelectedUsers = selectedUsers
        };
        
        return View(model);
    }
    
    [Authorize]
    [HttpPost]
    public async Task<IActionResult> CreateMTask(CreateMTaskViewModel model)
    {
       // if (!ModelState.IsValid) return View(model);

        var project = await _context.Projects
            .Include(x => x.MTasks)
            .FirstOrDefaultAsync(x => x.Id == model.idProject);

        if (project == null) return View(model);
        
        model.MTask.DateOfCreation = DateTime.Now;
        model.MTask.MTaskStatus = MTaskStatus.NEW;
        
        foreach (var item in model.SelectedUsers)
        {
            if (item.Value)
            {
                var u = await _context.Users
                    .Include(x => x.MTasks)
                    .FirstOrDefaultAsync(x => x.Email == item.Key);
                u!.MTasks.Add(model.MTask.Clone() as MTask ?? throw new InvalidOperationException());
                _context.Users.Update(u);
            }
        }
        
        project.MTasks.Add(model.MTask);

        _context.Projects.Update(project);
        await _context.SaveChangesAsync();
        
       
        return RedirectToAction("MainProjectPage", "Project", 
            new { idProject = model.idProject, isAdmin = model.IsAdmin });
    }

    [Authorize]
    public async Task<IActionResult> MainMTaskPage(int idTask)
    {
        var email = User.Identity?.Name;
        var user = await _context.Users.FirstOrDefaultAsync(x => x.Email == email);
        var task = await _context.MTasks.FirstOrDefaultAsync(x => x.Id == idTask);

        if (task!.MTaskStatus == MTaskStatus.NEW)
        {
            task!.MTaskStatus = MTaskStatus.IN_PROGRESS;
            _context.MTasks.Update(task);
            await _context.SaveChangesAsync();
        }
        
        var model = new MainMTaskPageViewModel
        {
            MTask = task
        };
        
        return View(model);
    }

    public async Task<IActionResult> SetMTaskStatus(int idTask, MTaskStatus status)
    {
        var task = await _context.MTasks.FirstOrDefaultAsync(x => x.Id == idTask);
        if (task != null)
        {
            task.MTaskStatus = status;
            _context.MTasks.Update(task);
            await _context.SaveChangesAsync();
        }
        
        return RedirectToAction("MainMTaskPage", "MTask", new { idTask = idTask });
    }
}