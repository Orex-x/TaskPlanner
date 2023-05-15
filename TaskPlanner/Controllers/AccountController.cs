using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TaskPlanner.Models;
using TaskPlanner.Services;

namespace TaskPlanner.Controllers;

public class AccountController : Controller
{
    private readonly DatabaseContext _context;

    public AccountController(DatabaseContext context)
    {
        _context = context;
    }
    
    public IActionResult Authorization() => View();
    public IActionResult Registration() => View();
   
    
    [Authorize]
    public async Task<IActionResult> AccountHome()
    {
        var email = User.Identity?.Name;
        var user = await _context.Users
            .Include(x => x.FavoriteMTasks)
            .FirstOrDefaultAsync(x => x.Email == email);
        
        var model = new AccountHomeViewModel
        {
            User = user!,
        };
            
        return View(model);
    }
    
    
    [HttpPost]
    public async Task<IActionResult> Registration(RegistrationViewModel model)
    {
        if (!ModelState.IsValid) return View(model);
        
        var findUser = await _context.Users.FirstOrDefaultAsync(u => u.Email == model.Email);

        if (findUser != null) return View(model);
        
        if (model.Password.Length > 7)
        {
            var hasher = new PasswordHasher<User>();

            var user = new User
            {
                FirstName = model.FirstName,
                SecondName = model.SecondName,
                LastName = model.LastName,
                Login = model.Login,
                Email = model.Email,
                Phone = model.Phone,
                PathToAvatar = "//images.wbstatic.net/img/0/medium/PersonalPhoto.png?2",
            };
            
            user.Password = hasher.HashPassword(user, model.Password);
            await Authenticate(user.Email);
            
                    
            await _context.Users.AddAsync(user);
            await  _context.SaveChangesAsync();
            return RedirectToAction("AccountHome", "Account");
        }
        ModelState.AddModelError("", "Пароль должен содержать минимум 8 символов");
        return View(model);
    }

    [HttpPost]
    public async Task<IActionResult> Authorization(AuthorizationViewModel model)
    {
        if (ModelState.IsValid)
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == model.Email);
            if (user != null)
            {
                var hasher = new PasswordHasher<User>();
                var s = hasher.VerifyHashedPassword(user, user.Password, model.Password);

                if (s == PasswordVerificationResult.Success)
                {
                    await Authenticate(user.Email);
                    return RedirectToAction("AccountHome", "Account");
                }
            }
            ModelState.AddModelError("", "Некорректные логин и(или) пароль");
        }
        return View(model);
    }

    private async Task Authenticate(string userName)
    {
        var claims = new List<Claim>
        {
            new (ClaimsIdentity.DefaultNameClaimType, userName)
        };

        var id = new ClaimsIdentity(claims, "ApplicationCookie", ClaimsIdentity.DefaultNameClaimType, ClaimsIdentity.DefaultRoleClaimType);
        await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, new ClaimsPrincipal(id));
    }

    public async Task<IActionResult> Logout()
    {
        await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
        return RedirectToAction("AccountHome", "Account");
    }

    [HttpPost]
    public async Task<IActionResult> UploadAvatar(IFormFile uploadedFile)
    {
        if (uploadedFile != null && uploadedFile.Length > 0)
        {
            // Генерируем уникальное имя файла
            var fileName = Guid.NewGuid() + Path.GetExtension(uploadedFile.FileName);

            // Создаем путь для сохранения файла
            var path = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot\\uploads", fileName);

            // Копируем содержимое файла в указанный поток
            using (var stream = new FileStream(path, FileMode.Create))
            {
                await uploadedFile.CopyToAsync(stream);
            }
            
            var email = User.Identity?.Name;
            var user = _context.Users.FirstOrDefault(x => x.Email == email);
            user!.PathToAvatar = $"~/uploads/{fileName}";
            _context.Users.Update(user);
            await _context.SaveChangesAsync();

        }

        return RedirectToAction("AccountHome", "Account");
    }
    
    [Authorize]
    public async Task<RedirectToActionResult> UpdateFavoriteMTask(int id)
    {
        var mTask = await _context.MTasks.FirstOrDefaultAsync(x => x.Id == id);
        
        if (mTask != null)
        {
            var email = User.Identity?.Name;
            var user = await _context.Users
                .Include(x => x.FavoriteMTasks)
                .FirstOrDefaultAsync(x => x.Email == email);
       
            if (user != null)
            {
                var _ = user.FavoriteMTasks.FirstOrDefault(x => x.Id == id);
                if (_ == null)
                    user.FavoriteMTasks.Add(mTask);
                else
                    user.FavoriteMTasks.Remove(mTask);   
                    
                _context.Users.Update(user);
                await _context.SaveChangesAsync();
            }
        }
        return RedirectToAction("AccountHome", "Account");
    }

}