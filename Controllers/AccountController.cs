using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using CookBook.Models;
using Microsoft.AspNetCore.Authorization;

namespace CookBook.Controllers;

public class AccountController : Controller
{
    private readonly UserManager<IdentityUser> _userManager;
    private readonly SignInManager<IdentityUser> _signInManager;

    public AccountController(UserManager<IdentityUser> userManager, SignInManager<IdentityUser> signInManager)
    {
        _userManager = userManager;
        _signInManager = signInManager;
    }
    
    private IActionResult RedirectToLocal(string? returnUrl)
    {
        if (Url.IsLocalUrl(returnUrl))
            return Redirect(returnUrl);
        
        return RedirectToAction("Index", "Home");
    }
    
    [HttpGet]
    public IActionResult Register()
    {
        return View();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Register(RegisterViewModel model)
    {
        if (ModelState.IsValid)
        {
            var user = new IdentityUser
            {
                UserName = model.Email,
                Email = model.Email,
            };
            
            var result = await _userManager.CreateAsync(user, model.Password);

            if (result.Succeeded)
            {
                await _signInManager.SignInAsync(user, false);
                return RedirectToAction("Index", "Home");
            }
            
            foreach (var error in result.Errors)
                ModelState.AddModelError(string.Empty, error.Description);
        }
        
        return View(model);
    }

    [HttpGet]
    public IActionResult Login(string? returnUrl = null)
    {
        ViewData["ReturnUrl"] = returnUrl;
        return View();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Login(LoginViewModel model, string? returnUrl = null)
    {
        ViewData["ReturnUrl"] = returnUrl;

        if (ModelState.IsValid)
        {
            var result = await _signInManager.PasswordSignInAsync(model.Email, model.Password, model.RememberMe, false);
            
            if (result.Succeeded)
                return RedirectToLocal(returnUrl);
            
            ModelState.AddModelError(string.Empty, "Неверный email или пароль");
        }
        
        return View(model);
    }

    [HttpGet]
    public async Task<IActionResult> Logout()
    {
        await _signInManager.SignOutAsync();
        return RedirectToAction("Index", "Home");
    }
    
    [Authorize]
    public async Task<IActionResult> Profile()
    {
        var user = await _userManager.GetUserAsync(User);
        if (user == null)
            return NotFound();
        
        return View(user);
    }

    [Authorize]
    public async Task<IActionResult> ChangePassword()
    {
        return View();
    }

    [Authorize]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ChangePassword(ChangePasswordViewModel model)
    {
        if (!ModelState.IsValid)
            return View(model);
        
        var user = await _userManager.GetUserAsync(User);
        if (user == null)
            return NotFound();

        var result = await _userManager.ChangePasswordAsync(user, model.CurrentPassword, model.NewPassword);
        if (result.Succeeded)
        {
            await _signInManager.RefreshSignInAsync(user);
            TempData["SuccessMessage"] = "Пароль успешно изменён!";
            return RedirectToAction(nameof(Profile));
        }
        
        foreach (var error in result.Errors)
            ModelState.AddModelError(string.Empty, error.Description);
        
        return View(model);
    }

    [Authorize]
    public async Task<IActionResult> ManageEmail()
    {
        var user = await _userManager.GetUserAsync(User);
        if (user == null)
            return NotFound();

        var model = new ManageEmailViewModel
        {
            CurrentEmail = user.Email,
            IsEmailConfirmed = await _userManager.IsEmailConfirmedAsync(user),
        };
        
        return View(model);
    }
    
    [HttpGet]
    public async Task<IActionResult> ConfirmEmail(string? userId, string? code)
    {
        if (userId == null || code == null)
        {
            TempData["ErrorMessage"] = "Неверная ссылка для подтверждения";
            return RedirectToAction(nameof(Login));
        }

        var user = await _userManager.FindByIdAsync(userId);
        if (user == null)
        {
            TempData["ErrorMessage"] = "Пользователь не найден";
            return RedirectToAction(nameof(Login));
        }

        var result = await _userManager.ConfirmEmailAsync(user, code);
    
        if (result.Succeeded)
        {
            await _signInManager.RefreshSignInAsync(user);
            TempData["SuccessMessage"] = "Email успешно подтверждён!";
            return RedirectToAction(nameof(Profile));
        }

        TempData["ErrorMessage"] = "Ошибка при подтверждении email";
        return RedirectToAction(nameof(Login));
    }

    [Authorize]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> SendEmailConfirmation()
    {
        var user = await _userManager.GetUserAsync(User);
        if (user == null)
        {
            return NotFound();
        }

        if (await _userManager.IsEmailConfirmedAsync(user))
        {
            TempData["InfoMessage"] = "Email уже подтверждён";
            return RedirectToAction(nameof(ManageEmail));
        }
        
        var token = await _userManager.GenerateEmailConfirmationTokenAsync(user);
        var callbackUrl = Url.Action(
            "ConfirmEmail",
            "Account",
            new { userId = user.Id, code = token },
            protocol: Request.Scheme);
        
        Console.WriteLine($"=== ПОДТВЕРЖДЕНИЕ EMAIL ===");
        Console.WriteLine($"Ссылка для подтверждения: {callbackUrl}");
        Console.WriteLine($"==========================");

        TempData["SuccessMessage"] = "Ссылка для подтверждения отправлена на ваш email";
        return RedirectToAction(nameof(ManageEmail));
    }
}