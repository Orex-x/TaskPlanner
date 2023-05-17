using System.Diagnostics;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TaskPlanner.Models;
using TaskPlanner.Services;

namespace TaskPlanner.Controllers;

public class HomeController : Controller
{
    private readonly DatabaseContext _context;

    public HomeController(DatabaseContext context)
    {
        _context = context;
    }

    [Authorize]
    public async Task<IActionResult> Index()
    {
        var email = User.Identity?.Name;

        var user = await _context.Users
            .Include(x => x.MTasks)
            .Include(x => x.FavoriteMTasks)
            .FirstOrDefaultAsync(x => x.Email == email);

        var list = user!.MTasks
            .Where(x => x.MTaskPriority == MTaskPriority.HIGH)
            .ToList();

        var model = new IndexViewModel
        {
            MTasksHighPreority = list,
            User = user,
        };

        return View(model);
    }

    [Authorize]
    public async Task<IActionResult> AllMTasks()
    {
        var email = User.Identity?.Name;

        var user = await _context.Users
            .Include(x => x.MTasks)
            .Include(x => x.FavoriteMTasks)
            .FirstOrDefaultAsync(x => x.Email == email);
        
        var model = new AllMTasksViewModel
        {
            MTasks = user!.MTasks,
            User = user
        };

        return View(model);
    }
    
    [HttpPost]
    
    public async Task<IActionResult> SearchMTasks(string? search)
    {
        var email = User.Identity?.Name;

        var user = await _context.Users
            .Include(x => x.MTasks)
            .Include(x => x.FavoriteMTasks)
            .FirstOrDefaultAsync(x => x.Email == email);

        var result = user.MTasks.Where(x => x.Title.ToLower()
            .Contains(search == null ? "" : search.ToLower())).ToList();

        var model = new SearchMTasksViewModel
        {
            User = user,
            MTasksResult = result
        };
        
        return View(model);
    }
    

    public IActionResult Privacy()
    {
        return View();
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}