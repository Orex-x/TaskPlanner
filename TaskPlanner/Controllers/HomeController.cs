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
            .FirstOrDefaultAsync(x => x.Email == email);

        var list = user!.MTasks
            .Where(x => x.MTaskPriority == MTaskPriority.HIGH)
            .ToList();

        var model = new IndexViewModel
        {
            MTasksHighPreority = list
        };

        return View(model);
    }

    [Authorize]
    public async Task<IActionResult> AllMTasks()
    {
        var email = User.Identity?.Name;

        var user = await _context.Users
            .Include(x => x.MTasks)
            .FirstOrDefaultAsync(x => x.Email == email);
        
        var model = new AllMTasksViewModel
        {
            MTasks = user!.MTasks
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