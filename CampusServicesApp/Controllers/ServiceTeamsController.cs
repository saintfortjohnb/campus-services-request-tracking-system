using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using CampusServicesApp.Models;

namespace CampusServicesApp.Controllers
{
    public class ServiceTeamsController : Controller
    {
        private readonly ApplicationDbContext _context;

        public ServiceTeamsController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: ServiceTeams
        public async Task<IActionResult> Index()
        {
            return View(await _context.ServiceTeams.ToListAsync());
        }

        // GET: ServiceTeams/Details/5
        public async Task<IActionResult> Details(int? id)
        {
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
            return View();
        }

        // POST: ServiceTeams/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("TeamId,TeamName,TeamEmail,IsActive")] ServiceTeam serviceTeam)
        {
            if (ModelState.IsValid)
            {
                _context.Add(serviceTeam);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(serviceTeam);
        }

        // GET: ServiceTeams/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
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
