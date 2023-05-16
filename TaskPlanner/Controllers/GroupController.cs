using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TaskPlanner.Models;
using TaskPlanner.Services;
using System.Windows;

namespace TaskPlanner.Controllers;

public class GroupController : Controller
{
    private readonly DatabaseContext _context;

    public GroupController(DatabaseContext context)
    {
        _context = context;
    }

    public IActionResult CreateGroup() => View();
    
    [Authorize]
    [HttpPost]
    public async Task<IActionResult> CreateGroup(CreateGroupViewModel model)
    {
        
        if (model.FileUpload?.FormFile != null && model.FileUpload.FormFile.Length > 0)
        {
            if (!ModelState.IsValid) return View(model);
            
            // Генерируем уникальное имя файла
            var fileName = Guid.NewGuid() + Path.GetExtension(model.FileUpload.FormFile.FileName);

            // Создаем путь для сохранения файла
            var path = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot\\uploads", fileName);

            // Копируем содержимое файла в указанный поток
            using (var stream = new FileStream(path, FileMode.Create))
            {
                await model.FileUpload.FormFile.CopyToAsync(stream);
            }
            
            model.Group!.PathToImage = $"~/uploads/{fileName}";
        }

        var email = User.Identity?.Name;

        var user = _context.Users.FirstOrDefault(x => x.Email == email);

        var userGroup = new UserGroup
        {
            Group = model.Group,
            User = user!,
            IsAdmin = true
        };

        await _context.UserGroups.AddAsync(userGroup);
        await _context.SaveChangesAsync();
        
        return RedirectToAction("MainGroupPage", "Group");
    }

    [Authorize]
    public async Task<IActionResult> GroupPage(int id)
    {
        var email = User.Identity?.Name;
        
        var userGroup = await _context.UserGroups
            .Include(x => x.Group)
            .ThenInclude(x => x.Projects)
            .Include(x => x.User)
            .Where(x => x.User.Email == email)
            .FirstOrDefaultAsync(x => x.Group.Id == id);


        var model = new GroupPageViewModel
        {
            UserGroup = userGroup!,
        };

        return View(model);
    }

    [Authorize]
    public async Task<IActionResult> InviteLink(string link)
    {
        int idGroup = Convert.ToInt32(link.Substring(16));
        
        var email = User.Identity?.Name;

        var user = _context.Users.FirstOrDefault(x => x.Email == email);

        var group = await _context.Groups.FirstOrDefaultAsync(x => x.Id == idGroup);

        var userGroup = new UserGroup
        {
            Group = group!,
            User = user!,
            IsAdmin = false
        };

        await _context.UserGroups.AddAsync(userGroup);
        await _context.SaveChangesAsync();
        
        return RedirectToAction("MainGroupPage", "Group");
    }
    
    
    [Authorize]
    public async Task<IActionResult> UsersInGroup(int id, bool admin)
    {
        var userGroups = await _context.UserGroups
            .Where(x => x.Group.Id == id)
            .Include(x => x.Group)
            .Include(x => x.User).ToListAsync();


        var inviteLink = $"LinkGroupInvite-{id}";
        
        var model = new UsersInGroupViewModel
        {
            UserGroups = userGroups,
            IsAdmin = admin,
            InviteLink = inviteLink
        };

        return View(model);
    }
    
    [Authorize]
    public async Task<IActionResult> RemoveUserGroup(int id)
    {
        var userGroup = await _context.UserGroups.FirstOrDefaultAsync(x => x.Id == id);
        
        _context.UserGroups.Remove(userGroup!);
        
        await _context.SaveChangesAsync();
        
        return RedirectToAction("MainGroupPage", "Group");
    }

    [Authorize]
    public async Task<IActionResult> DeleteGroup(int id)
    {
        var userGroup = await _context.UserGroups
            .Include(x => x.Group)
            .FirstOrDefaultAsync(x => x.Id == id);

        _context.Groups.Remove(userGroup!.Group);

        await _context.SaveChangesAsync();
        
        return RedirectToAction("MainGroupPage", "Group");
    }
    
    
    [Authorize]
    public async Task<IActionResult> MainGroupPage()
    {
        var email = User.Identity?.Name;

        var user = _context.Users.FirstOrDefault(x => x.Email == email);

        var list = await _context.UserGroups.Where(x => x.User.Id == user!.Id)
            .Include(x => x.Group)
            .ThenInclude(x => x.Projects)
            .Include(x => x.User)
            .ToListAsync();
        
        var model = new MainGroupPageViewModel
        {
            UserGroups = list
        };
        
        return View(model);
    }
}