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
    public class CategoriesController : Controller
    {
        private readonly ApplicationDbContext _context;

        public CategoriesController(ApplicationDbContext context)
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

        // GET: Categories
        public async Task<IActionResult> Index()
        {
            if (!IsLoggedIn())
            {
                return RedirectToAction("Login", "Account");
            }

            if (!HasRole("Admin", "Manager"))
            {
                return RedirectToAction("Index", "Home");
            }

            var applicationDbContext = _context.Categories.Include(c => c.DefaultTeam);
            return View(await applicationDbContext.ToListAsync());
        }

        // GET: Categories/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (!IsLoggedIn())
            {
                return RedirectToAction("Login", "Account");
            }

            if (!HasRole("Admin", "Manager"))
            {
                return RedirectToAction("Index", "Home");
            }

            if (id == null)
            {
                return NotFound();
            }

            var category = await _context.Categories
                .Include(c => c.DefaultTeam)
                .FirstOrDefaultAsync(m => m.CategoryId == id);
            if (category == null)
            {
                return NotFound();
            }

            return View(category);
        }

        // GET: Categories/Create
        public IActionResult Create()
        {
            if (!IsLoggedIn())
            {
                return RedirectToAction("Login", "Account");
            }

            if (!HasRole("Admin", "Manager"))
            {
                return RedirectToAction("Index", "Home");
            }

            ViewData["DefaultTeamId"] = new SelectList(_context.ServiceTeams, "TeamId", "TeamName");
            return View();
        }

        // POST: Categories/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("CategoryName,DefaultTeamId,IsActive")] Category category)
        {
            if (!IsLoggedIn())
            {
                return RedirectToAction("Login", "Account");
            }

            if (!HasRole("Admin", "Manager"))
            {
                return RedirectToAction("Index", "Home");
            }

            ModelState.Remove(nameof(Category.DefaultTeam));

            if (ModelState.IsValid)
            {
                _context.Add(category);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }

            ViewData["DefaultTeamId"] = new SelectList(_context.ServiceTeams, "TeamId", "TeamName", category.DefaultTeamId);
            return View(category);
        }

        // GET: Categories/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (!IsLoggedIn())
            {
                return RedirectToAction("Login", "Account");
            }

            if (!HasRole("Admin", "Manager"))
            {
                return RedirectToAction("Index", "Home");
            }

            if (id == null)
            {
                return NotFound();
            }

            var category = await _context.Categories.FindAsync(id);
            if (category == null)
            {
                return NotFound();
            }

            ViewData["DefaultTeamId"] = new SelectList(_context.ServiceTeams, "TeamId", "TeamName", category.DefaultTeamId);
            return View(category);
        }

        // POST: Categories/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("CategoryId,CategoryName,DefaultTeamId,IsActive")] Category category)
        {
            if (!IsLoggedIn())
            {
                return RedirectToAction("Login", "Account");
            }

            if (!HasRole("Admin", "Manager"))
            {
                return RedirectToAction("Index", "Home");
            }

            if (id != category.CategoryId)
            {
                return NotFound();
            }

            ModelState.Remove(nameof(Category.DefaultTeam));

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(category);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!CategoryExists(category.CategoryId))
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

            ViewData["DefaultTeamId"] = new SelectList(_context.ServiceTeams, "TeamId", "TeamName", category.DefaultTeamId);
            return View(category);
        }

        // GET: Categories/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (!IsLoggedIn())
            {
                return RedirectToAction("Login", "Account");
            }

            if (!HasRole("Admin", "Manager"))
            {
                return RedirectToAction("Index", "Home");
            }

            if (id == null)
            {
                return NotFound();
            }

            var category = await _context.Categories
                .Include(c => c.DefaultTeam)
                .FirstOrDefaultAsync(m => m.CategoryId == id);
            if (category == null)
            {
                return NotFound();
            }

            return View(category);
        }

        // POST: Categories/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            if (!IsLoggedIn())
            {
                return RedirectToAction("Login", "Account");
            }

            if (!HasRole("Admin", "Manager"))
            {
                return RedirectToAction("Index", "Home");
            }

            var category = await _context.Categories.FindAsync(id);
            if (category != null)
            {
                _context.Categories.Remove(category);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool CategoryExists(int id)
        {
            return _context.Categories.Any(e => e.CategoryId == id);
        }
    }
}