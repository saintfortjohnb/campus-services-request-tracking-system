using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using CampusServicesApp.Models;

namespace CampusServicesApp.Controllers
{
    public class StatusHistoriesController : Controller
    {
        private readonly ApplicationDbContext _context;

        public StatusHistoriesController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: StatusHistories
        public async Task<IActionResult> Index()
        {
            if (!HttpContext.Session.GetInt32("UserId").HasValue)
            {
                return RedirectToAction("Login", "Account");
            }

            var roleName = HttpContext.Session.GetString("RoleName")?.Trim();
            var userId = HttpContext.Session.GetInt32("UserId");

            var query = _context.StatusHistories
                .Include(s => s.ChangedByNavigation)
                .Include(s => s.Request)
                .AsQueryable();

            if (string.Equals(roleName, "Requester", StringComparison.OrdinalIgnoreCase) && userId.HasValue)
            {
                query = query.Where(s => s.Request.RequesterId == userId.Value);
            }

            var applicationDbContext = await query
                .OrderBy(s => s.Request.TrackingNumber)
                .ThenBy(s => s.ChangedAt)
                .ToListAsync();

            return View(applicationDbContext);
        }

        // GET: StatusHistories/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (!HttpContext.Session.GetInt32("UserId").HasValue)
            {
                return RedirectToAction("Login", "Account");
            }

            if (id == null)
            {
                return NotFound();
            }

            var statusHistory = await _context.StatusHistories
                .Include(s => s.ChangedByNavigation)
                .Include(s => s.Request)
                .FirstOrDefaultAsync(m => m.StatusHistoryId == id);

            if (statusHistory == null)
            {
                return NotFound();
            }

            return View(statusHistory);
        }

        // GET: StatusHistories/Create
        public IActionResult Create()
        {
            if (!HttpContext.Session.GetInt32("UserId").HasValue)
            {
                return RedirectToAction("Login", "Account");
            }

            var roleName = HttpContext.Session.GetString("RoleName")?.Trim();
            if (!string.Equals(roleName, "Admin", StringComparison.OrdinalIgnoreCase) &&
                !string.Equals(roleName, "Manager", StringComparison.OrdinalIgnoreCase))
            {
                return RedirectToAction("Index", "Home");
            }

            ViewData["ChangedBy"] = new SelectList(_context.Users, "UserId", "Name");
            ViewData["RequestId"] = new SelectList(_context.ServiceRequests, "RequestId", "TrackingNumber");
            return View();
        }

        // POST: StatusHistories/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("RequestId,OldStatus,NewStatus,ChangedBy,ChangedAt")] StatusHistory statusHistory)
        {
            if (!HttpContext.Session.GetInt32("UserId").HasValue)
            {
                return RedirectToAction("Login", "Account");
            }

            var roleName = HttpContext.Session.GetString("RoleName")?.Trim();
            if (!string.Equals(roleName, "Admin", StringComparison.OrdinalIgnoreCase) &&
                !string.Equals(roleName, "Manager", StringComparison.OrdinalIgnoreCase))
            {
                return RedirectToAction("Index", "Home");
            }

            ModelState.Remove(nameof(StatusHistory.Request));
            ModelState.Remove(nameof(StatusHistory.ChangedByNavigation));

            if (ModelState.IsValid)
            {
                _context.Add(statusHistory);

                var serviceRequest = await _context.ServiceRequests.FindAsync(statusHistory.RequestId);
                if (serviceRequest != null)
                {
                    serviceRequest.CurrentStatus = statusHistory.NewStatus;

                    if (string.Equals(statusHistory.NewStatus, "Closed", StringComparison.OrdinalIgnoreCase))
                    {
                        serviceRequest.ClosedAt = DateTime.Now;
                    }
                    else
                    {
                        serviceRequest.ClosedAt = null;
                    }
                }

                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }

            ViewData["ChangedBy"] = new SelectList(_context.Users, "UserId", "Name", statusHistory.ChangedBy);
            ViewData["RequestId"] = new SelectList(_context.ServiceRequests, "RequestId", "TrackingNumber", statusHistory.RequestId);
            return View(statusHistory);
        }

        // GET: StatusHistories/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (!HttpContext.Session.GetInt32("UserId").HasValue)
            {
                return RedirectToAction("Login", "Account");
            }

            var roleName = HttpContext.Session.GetString("RoleName")?.Trim();
            if (!string.Equals(roleName, "Admin", StringComparison.OrdinalIgnoreCase) &&
                !string.Equals(roleName, "Manager", StringComparison.OrdinalIgnoreCase))
            {
                return RedirectToAction("Index", "Home");
            }

            if (id == null)
            {
                return NotFound();
            }

            var statusHistory = await _context.StatusHistories.FindAsync(id);
            if (statusHistory == null)
            {
                return NotFound();
            }

            ViewData["ChangedBy"] = new SelectList(_context.Users, "UserId", "Name", statusHistory.ChangedBy);
            ViewData["RequestId"] = new SelectList(_context.ServiceRequests, "RequestId", "TrackingNumber", statusHistory.RequestId);
            return View(statusHistory);
        }

        // POST: StatusHistories/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("StatusHistoryId,RequestId,OldStatus,NewStatus,ChangedBy,ChangedAt")] StatusHistory statusHistory)
        {
            if (!HttpContext.Session.GetInt32("UserId").HasValue)
            {
                return RedirectToAction("Login", "Account");
            }

            var roleName = HttpContext.Session.GetString("RoleName")?.Trim();
            if (!string.Equals(roleName, "Admin", StringComparison.OrdinalIgnoreCase) &&
                !string.Equals(roleName, "Manager", StringComparison.OrdinalIgnoreCase))
            {
                return RedirectToAction("Index", "Home");
            }

            if (id != statusHistory.StatusHistoryId)
            {
                return NotFound();
            }

            ModelState.Remove(nameof(StatusHistory.Request));
            ModelState.Remove(nameof(StatusHistory.ChangedByNavigation));

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(statusHistory);

                    var serviceRequest = await _context.ServiceRequests.FindAsync(statusHistory.RequestId);
                    if (serviceRequest != null)
                    {
                        serviceRequest.CurrentStatus = statusHistory.NewStatus;

                        if (string.Equals(statusHistory.NewStatus, "Closed", StringComparison.OrdinalIgnoreCase))
                        {
                            serviceRequest.ClosedAt = DateTime.Now;
                        }
                        else
                        {
                            serviceRequest.ClosedAt = null;
                        }
                    }

                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!StatusHistoryExists(statusHistory.StatusHistoryId))
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

            ViewData["ChangedBy"] = new SelectList(_context.Users, "UserId", "Name", statusHistory.ChangedBy);
            ViewData["RequestId"] = new SelectList(_context.ServiceRequests, "RequestId", "TrackingNumber", statusHistory.RequestId);
            return View(statusHistory);
        }

        // GET: StatusHistories/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (!HttpContext.Session.GetInt32("UserId").HasValue)
            {
                return RedirectToAction("Login", "Account");
            }

            var roleName = HttpContext.Session.GetString("RoleName")?.Trim();
            if (!string.Equals(roleName, "Admin", StringComparison.OrdinalIgnoreCase) &&
                !string.Equals(roleName, "Manager", StringComparison.OrdinalIgnoreCase))
            {
                return RedirectToAction("Index", "Home");
            }

            if (id == null)
            {
                return NotFound();
            }

            var statusHistory = await _context.StatusHistories
                .Include(s => s.ChangedByNavigation)
                .Include(s => s.Request)
                .FirstOrDefaultAsync(m => m.StatusHistoryId == id);

            if (statusHistory == null)
            {
                return NotFound();
            }

            return View(statusHistory);
        }

        // POST: StatusHistories/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            if (!HttpContext.Session.GetInt32("UserId").HasValue)
            {
                return RedirectToAction("Login", "Account");
            }

            var roleName = HttpContext.Session.GetString("RoleName")?.Trim();
            if (!string.Equals(roleName, "Admin", StringComparison.OrdinalIgnoreCase) &&
                !string.Equals(roleName, "Manager", StringComparison.OrdinalIgnoreCase))
            {
                return RedirectToAction("Index", "Home");
            }

            var statusHistory = await _context.StatusHistories.FindAsync(id);
            if (statusHistory != null)
            {
                _context.StatusHistories.Remove(statusHistory);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool StatusHistoryExists(int id)
        {
            return _context.StatusHistories.Any(e => e.StatusHistoryId == id);
        }
    }
}