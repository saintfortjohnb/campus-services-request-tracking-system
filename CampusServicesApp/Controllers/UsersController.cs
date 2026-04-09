using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using CampusServicesApp.Models;
using Microsoft.AspNetCore.Http;

namespace CampusServicesApp.Controllers
{
    public class UsersController : Controller
    {
        private readonly ApplicationDbContext _context;

        public UsersController(ApplicationDbContext context)
        {
            _context = context;
        }

        private int? GetCurrentUserId()
        {
            return HttpContext.Session.GetInt32("UserId");
        }

        private bool IsLoggedIn()
        {
            return GetCurrentUserId().HasValue;
        }

        private bool HasRole(params string[] roles)
        {
            var roleName = HttpContext.Session.GetString("RoleName")?.Trim();
            return !string.IsNullOrWhiteSpace(roleName) &&
                   roles.Any(r => string.Equals(r, roleName, StringComparison.OrdinalIgnoreCase));
        }

        // GET: Users
        public async Task<IActionResult> Index()
        {
            if (!IsLoggedIn())
            {
                return RedirectToAction("Login", "Account");
            }

            if (!HasRole("Admin"))
            {
                return RedirectToAction("Index", "Home");
            }

            var applicationDbContext = _context.Users.Include(u => u.Role);
            return View(await applicationDbContext.ToListAsync());
        }

        // GET: Users/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (!IsLoggedIn())
            {
                return RedirectToAction("Login", "Account");
            }

            if (!HasRole("Admin"))
            {
                return RedirectToAction("Index", "Home");
            }

            if (id == null)
            {
                return NotFound();
            }

            var user = await _context.Users
                .Include(u => u.Role)
                .FirstOrDefaultAsync(m => m.UserId == id);
            if (user == null)
            {
                return NotFound();
            }

            return View(user);
        }

        // GET: Users/Create
        public IActionResult Create()
        {
            if (!IsLoggedIn())
            {
                return RedirectToAction("Login", "Account");
            }

            if (!HasRole("Admin"))
            {
                return RedirectToAction("Index", "Home");
            }

            ViewData["RoleId"] = new SelectList(_context.Roles, "RoleId", "RoleName");
            return View();
        }

        // POST: Users/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Name,Email,RoleId")] User user)
        {
            if (!IsLoggedIn())
            {
                return RedirectToAction("Login", "Account");
            }

            if (!HasRole("Admin"))
            {
                return RedirectToAction("Index", "Home");
            }

            ModelState.Remove(nameof(CampusServicesApp.Models.User.Role));

            if (ModelState.IsValid)
            {
                _context.Add(user);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            ViewData["RoleId"] = new SelectList(_context.Roles, "RoleId", "RoleName", user.RoleId);
            return View(user);
        }

        // GET: Users/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (!IsLoggedIn())
            {
                return RedirectToAction("Login", "Account");
            }

            if (!HasRole("Admin"))
            {
                return RedirectToAction("Index", "Home");
            }

            if (id == null)
            {
                return NotFound();
            }

            var user = await _context.Users.FindAsync(id);
            if (user == null)
            {
                return NotFound();
            }
            ViewData["RoleId"] = new SelectList(_context.Roles, "RoleId", "RoleName", user.RoleId);
            return View(user);
        }

        // POST: Users/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("UserId,Name,Email,RoleId")] User user)
        {
            if (!IsLoggedIn())
            {
                return RedirectToAction("Login", "Account");
            }

            if (!HasRole("Admin"))
            {
                return RedirectToAction("Index", "Home");
            }

            if (id != user.UserId)
            {
                return NotFound();
            }

            ModelState.Remove(nameof(CampusServicesApp.Models.User.Role));

            if (ModelState.IsValid)
            {
                var existingUser = await _context.Users.FindAsync(id);
                if (existingUser == null)
                {
                    return NotFound();
                }

                existingUser.Name = user.Name;
                existingUser.Email = user.Email;
                existingUser.RoleId = user.RoleId;

                try
                {
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!UserExists(user.UserId))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }
            ViewData["RoleId"] = new SelectList(_context.Roles, "RoleId", "RoleName", user.RoleId);
            return View(user);
        }

        // GET: Users/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (!IsLoggedIn())
            {
                return RedirectToAction("Login", "Account");
            }

            if (!HasRole("Admin"))
            {
                return RedirectToAction("Index", "Home");
            }

            if (id == null)
            {
                return NotFound();
            }

            var user = await _context.Users
                .Include(u => u.Role)
                .FirstOrDefaultAsync(m => m.UserId == id);
            if (user == null)
            {
                return NotFound();
            }

            return View(user);
        }

        // POST: Users/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            if (!IsLoggedIn())
            {
                return RedirectToAction("Login", "Account");
            }

            if (!HasRole("Admin"))
            {
                return RedirectToAction("Index", "Home");
            }

            var user = await _context.Users
                .Include(u => u.Role)
                .FirstOrDefaultAsync(u => u.UserId == id);

            if (user == null)
            {
                return RedirectToAction(nameof(Index));
            }

            var isRequester = await _context.ServiceRequests.AnyAsync(r => r.RequesterId == id);
            var isTechnician = await _context.Assignments.AnyAsync(a => a.TechnicianId == id);
            var isAssignedBy = await _context.Assignments.AnyAsync(a => a.AssignedBy == id);
            var changedStatus = await _context.StatusHistories.AnyAsync(s => s.ChangedBy == id);

            if (isRequester || isTechnician || isAssignedBy || changedStatus)
            {
                ModelState.AddModelError(string.Empty, "This user cannot be deleted because they are still linked to service requests, assignments, or status history records.");
                return View("Delete", user);
            }

            _context.Users.Remove(user);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool UserExists(int id)
        {
            return _context.Users.Any(e => e.UserId == id);
        }
    }
}