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
        var email = User.Identity?.Name;
        
        var users = _context.UserGroups
            .Where(x => x.Group.Id == idGroup)
            .Select(x => x.User);

        var selectedUsers = new Dictionary<string, Boolean>();
        
        await users.ForEachAsync(user =>
        {
            if (user.Email != email)
                selectedUsers.Add(user.Email, true);
        });

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
       
       var email = User.Identity?.Name;
       var author = await _context.Users.FirstOrDefaultAsync(x => x.Email == email);

       var project = await _context.Projects
           .Include(x => x.MTasks)
           .FirstOrDefaultAsync(x => x.Id == model.idProject);
        
       if (project == null) return View(model);
        
       model.MTask.DateOfCreation = DateTime.Now;
       model.MTask.MTaskStatus = MTaskStatus.NEW; 
       
       model.MTask.Author = author!;

       model.MTask.Code = Guid.NewGuid().ToString();
       
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
        
        var task = await _context.MTasks
            .Include(x => x.Author)
            .FirstOrDefaultAsync(x => x.Id == idTask);

        if (task!.MTaskStatus == MTaskStatus.NEW)
        {
            task!.MTaskStatus = MTaskStatus.IN_PROGRESS;
            _context.MTasks.Update(task);
            await _context.SaveChangesAsync();
        }
        
        var model = new MainMTaskPageViewModel
        {
            MTask = task,
            isAuthor = task.Author.Email == email,
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

    public async Task<IActionResult> DeleteTask(int idTask)
    {
        var task = await _context.MTasks.FirstOrDefaultAsync(x => x.Id == idTask);

        if (task != null)
        {

            var list = _context.MTasks.Where(x => x.Code == task.Code);

            await list.ForEachAsync(x => _context.MTasks.Remove(x));

            await _context.SaveChangesAsync();
        }
        
        return RedirectToAction("AllMTasks", "Home");
    }
    
}