using FiorelloBack.Models;
using FiorelloBack.ViewModels;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace FiorelloBack.Areas.Manage.Controllers
{
    [Area("Manage")]
    public class AccountController : Controller
    {
            private readonly UserManager<AppUser> _userManager;
            private readonly RoleManager<IdentityRole> _roleManager;
            private readonly SignInManager<AppUser> _signInResult;

            public AccountController(UserManager<AppUser> userManager, RoleManager<IdentityRole> roleManager, SignInManager<AppUser> signInResult)
            {
                _userManager = userManager;
                _roleManager = roleManager;
                _signInResult = signInResult;
            }
            public IActionResult Login()
            {
                return View();
            }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginVM login)
        {
            if (!ModelState.IsValid) return View();

            AppUser user = await _userManager.FindByNameAsync(login.Username);

            if (user == null)
            {
                ModelState.AddModelError("", "Username or password is incorrect");
                return View();
            }
            if (!user.IsAdmin)
            {
                ModelState.AddModelError("", "Username or password is incorrect");
                return View();
            }

            Microsoft.AspNetCore.Identity.SignInResult result = await _signInResult.PasswordSignInAsync(user, login.Password, false, false);
            if (!result.Succeeded)
            {
                ModelState.AddModelError("", "Username or password is incorrect");
                return View();
            }
            return RedirectToAction("index", "dashboard");

        }


    }
    }
