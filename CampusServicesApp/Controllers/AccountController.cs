using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using CampusServicesApp.Models;

namespace CampusServicesApp.Controllers
{
    public class AccountController : Controller
    {
        private readonly ApplicationDbContext _context;

        public AccountController(ApplicationDbContext context)
        {
            _context = context;
        }

        private bool IsLoggedIn()
        {
            return HttpContext.Session.GetInt32("UserId").HasValue;
        }

        // GET: Account/Login
        public IActionResult Login()
        {
            if (IsLoggedIn())
            {
                return RedirectToAction("Index", "Home");
            }

            return View();
        }

        // POST: Account/Login
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(string? email)
        {
            if (IsLoggedIn())
            {
                return RedirectToAction("Index", "Home");
            }

            email = email?.Trim();

            if (string.IsNullOrWhiteSpace(email))
            {
                ModelState.AddModelError(string.Empty, "Please enter your email.");
                return View();
            }

            var user = await _context.Users
                .Include(u => u.Role)
                .FirstOrDefaultAsync(u => u.Email == email);

            if (user == null)
            {
                ModelState.AddModelError(string.Empty, "No user found with that email.");
                return View();
            }

            HttpContext.Session.Clear();

            HttpContext.Session.SetInt32("UserId", user.UserId);
            HttpContext.Session.SetString("UserName", user.Name?.Trim() ?? string.Empty);
            HttpContext.Session.SetString("UserEmail", user.Email?.Trim() ?? string.Empty);
            HttpContext.Session.SetInt32("RoleId", user.RoleId);
            HttpContext.Session.SetString("RoleName", user.Role?.RoleName?.Trim() ?? string.Empty);

            return RedirectToAction("Index", "Home");
        }

        // GET: Account/Logout
        public IActionResult Logout()
        {
            HttpContext.Session.Clear();
            return RedirectToAction("Index", "Home");
        }
    }
}