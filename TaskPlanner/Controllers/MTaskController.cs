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
                selectedUsers.Add(string.Join(":", new string[] {user.Email, user.PathToAvatar}), true);
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

       var project = await _context.Projects
           .Include(x => x.MTasks)
           .FirstOrDefaultAsync(x => x.Id == model.idProject);
        
       if (project == null) return View(model);
        
       model.MTask.DateOfCreation = DateTime.Now;
       model.MTask.MTaskStatus = MTaskStatus.NEW; 
       
       model.MTask.AuthorEmail = email!;

       model.MTask.Code = Guid.NewGuid().ToString();
       
       foreach (var item in model.SelectedUsers)
       {
           if (item.Value)
           {
               var itemEmail = item.Key.Split(":")[0];
               var u = await _context.Users
                   .Include(x => x.MTasks)
                   .FirstOrDefaultAsync(x => x.Email == itemEmail);
               var t = model.MTask.Clone() as MTask ?? throw new InvalidOperationException();
               u!.MTasks.Add(t);
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
            .FirstOrDefaultAsync(x => x.Id == idTask);

        if (task!.MTaskStatus == MTaskStatus.NEW)
        {
            task!.MTaskStatus = MTaskStatus.IN_PROGRESS;
            _context.MTasks.Update(task);
            await _context.SaveChangesAsync();
        }
        
        var author = _context.Users.FirstOrDefault(x => x.Email == task.AuthorEmail);

        var model = new MainMTaskPageViewModel
        {
            MTask = task,
            isAuthor = task.AuthorEmail == email,
            Author = author!
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

    public async Task<IActionResult> TaskPerformers(int idTask, bool completed)
    {
        var mainTask = await _context.MTasks
            .FirstOrDefaultAsync(x => x.Id == idTask);

        var users = _context.Users
            .Include(x => x.MTasks)
            .Where(x => x.MTasks.Select(y => y.Code).Contains(mainTask!.Code));

        var list = new List<UserTask>();
        
        await users.ForEachAsync(user =>
        {
            var task = user.MTasks.FirstOrDefault(x => x.Code == mainTask!.Code);

            if (completed)
            {
                if (task!.MTaskStatus == MTaskStatus.SHIPPED || task!.MTaskStatus == MTaskStatus.COMPLETED)
                {
                    list.Add(new UserTask
                    {
                        User = user,
                        MTask = task,
                    });
                }
            }
            else
            {
                if (task!.MTaskStatus != MTaskStatus.SHIPPED && task!.MTaskStatus != MTaskStatus.COMPLETED)
                {
                    list.Add(new UserTask
                    {
                        User = user,
                        MTask = task,
                    });
                }
            }
        });

        var viewModel = new TaskPerformersViewModel
        { 
            UserTasks = list
        };
        
        return View(viewModel);
    }
}