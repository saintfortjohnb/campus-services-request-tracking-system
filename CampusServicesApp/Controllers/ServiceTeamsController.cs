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
    public class ServiceTeamsController : Controller
    {
        private readonly ApplicationDbContext _context;

        public ServiceTeamsController(ApplicationDbContext context)
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

        // GET: ServiceTeams
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

            return View(await _context.ServiceTeams.ToListAsync());
        }

        // GET: ServiceTeams/Details/5
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

            var serviceTeam = await _context.ServiceTeams
                .FirstOrDefaultAsync(m => m.TeamId == id);
            if (serviceTeam == null)
            {
                return NotFound();
            }

            return View(serviceTeam);
        }

        // GET: ServiceTeams/Create
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

            return View();
        }

        // POST: ServiceTeams/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("TeamName,TeamEmail,IsActive")] ServiceTeam serviceTeam)
        {
            if (!IsLoggedIn())
            {
                return RedirectToAction("Login", "Account");
            }

            if (!HasRole("Admin", "Manager"))
            {
                return RedirectToAction("Index", "Home");
            }

            if (string.IsNullOrWhiteSpace(serviceTeam.TeamName))
            {
                ModelState.AddModelError(nameof(ServiceTeam.TeamName), "Team name is required.");
            }

            if (string.IsNullOrWhiteSpace(serviceTeam.TeamEmail))
            {
                ModelState.AddModelError(nameof(ServiceTeam.TeamEmail), "Team email is required.");
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Add(serviceTeam);
                    await _context.SaveChangesAsync();
                    return RedirectToAction(nameof(Index));
                }
                catch (DbUpdateException ex)
                {
                    var message = ex.InnerException?.Message ?? ex.Message;

                    if (message.Contains("duplicate", StringComparison.OrdinalIgnoreCase) ||
                        message.Contains("unique", StringComparison.OrdinalIgnoreCase) ||
                        message.Contains("UQ__", StringComparison.OrdinalIgnoreCase))
                    {
                        ModelState.AddModelError(nameof(ServiceTeam.TeamName), "A service team with this name already exists. Use a different team name or edit the existing team.");
                    }
                    else
                    {
                        ModelState.AddModelError(string.Empty, "Unable to save the service team right now. Please try again.");
                    }
                }
            }

            return View(serviceTeam);
        }

        // GET: ServiceTeams/Edit/5
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

            var serviceTeam = await _context.ServiceTeams.FindAsync(id);
            if (serviceTeam == null)
            {
                return NotFound();
            }

            return View(serviceTeam);
        }

        // POST: ServiceTeams/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("TeamId,TeamName,TeamEmail,IsActive")] ServiceTeam serviceTeam)
        {
            if (!IsLoggedIn())
            {
                return RedirectToAction("Login", "Account");
            }

            if (!HasRole("Admin", "Manager"))
            {
                return RedirectToAction("Index", "Home");
            }

            if (id != serviceTeam.TeamId)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(serviceTeam);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!ServiceTeamExists(serviceTeam.TeamId))
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

            return View(serviceTeam);
        }

        // GET: ServiceTeams/Delete/5
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

            var serviceTeam = await _context.ServiceTeams
                .FirstOrDefaultAsync(m => m.TeamId == id);
            if (serviceTeam == null)
            {
                return NotFound();
            }

            return View(serviceTeam);
        }

        // POST: ServiceTeams/Delete/5
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

            var serviceTeam = await _context.ServiceTeams.FindAsync(id);
            if (serviceTeam != null)
            {
                _context.ServiceTeams.Remove(serviceTeam);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool ServiceTeamExists(int id)
        {
            return _context.ServiceTeams.Any(e => e.TeamId == id);
        }
    }
}