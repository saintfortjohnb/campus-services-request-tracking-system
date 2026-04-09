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
    public class AssignmentsController : Controller
    {
        private readonly ApplicationDbContext _context;

        public AssignmentsController(ApplicationDbContext context)
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

        // GET: Assignments
        public async Task<IActionResult> Index()
        {
            var currentUserId = GetCurrentUserId();

            if (!IsLoggedIn() || !currentUserId.HasValue)
            {
                return RedirectToAction("Login", "Account");
            }

            var userId = currentUserId.Value;

            if (!HasRole("Technician", "Manager", "Admin"))
            {
                return RedirectToAction("Index", "Home");
            }

            var query = _context.Assignments
                .Include(a => a.AssignedByNavigation)
                .Include(a => a.Request)
                .Include(a => a.Technician)
                .AsQueryable();

            if (HasRole("Technician") && !HasRole("Manager", "Admin"))
            {
                query = query.Where(a => a.TechnicianId == userId);
            }

            return View(await query.ToListAsync());
        }

        // GET: Assignments/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            var currentUserId = GetCurrentUserId();

            if (!IsLoggedIn() || !currentUserId.HasValue)
            {
                return RedirectToAction("Login", "Account");
            }

            var userId = currentUserId.Value;

            if (!HasRole("Technician", "Manager", "Admin"))
            {
                return RedirectToAction("Index", "Home");
            }

            if (id == null)
            {
                return NotFound();
            }

            var assignment = await _context.Assignments
                .Include(a => a.AssignedByNavigation)
                .Include(a => a.Request)
                .Include(a => a.Technician)
                .FirstOrDefaultAsync(m => m.AssignmentId == id);

            if (assignment == null)
            {
                return NotFound();
            }

            if (HasRole("Technician") && !HasRole("Manager", "Admin") &&
                assignment.TechnicianId != userId)
            {
                return RedirectToAction("Index", "Home");
            }

            return View(assignment);
        }

        // GET: Assignments/Create
        public IActionResult Create()
        {
            var currentUserId = GetCurrentUserId();

            if (!IsLoggedIn() || !currentUserId.HasValue)
            {
                return RedirectToAction("Login", "Account");
            }

            var userId = currentUserId.Value;

            if (!HasRole("Manager", "Admin"))
            {
                return RedirectToAction("Index", "Home");
            }

            ViewData["AssignedBy"] = new SelectList(
                _context.Users.Where(u => u.UserId == userId),
                "UserId",
                "Name",
                userId);

            ViewData["RequestId"] = new SelectList(_context.ServiceRequests, "RequestId", "TrackingNumber");
            ViewData["TechnicianId"] = new SelectList(_context.Users, "UserId", "Name");

            return View();
        }

        // POST: Assignments/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("RequestId,TechnicianId,AssignedBy,AssignedAt")] Assignment assignment)
        {
            var currentUserId = GetCurrentUserId();

            if (!IsLoggedIn() || !currentUserId.HasValue)
            {
                return RedirectToAction("Login", "Account");
            }

            var userId = currentUserId.Value;

            if (!HasRole("Manager", "Admin"))
            {
                return RedirectToAction("Index", "Home");
            }

            assignment.AssignedBy = userId;

            ModelState.Remove(nameof(Assignment.Request));
            ModelState.Remove(nameof(Assignment.Technician));
            ModelState.Remove(nameof(Assignment.AssignedByNavigation));

            if (ModelState.IsValid)
            {
                assignment.AssignedAt = DateTime.Now;
                _context.Add(assignment);

                var serviceRequest = await _context.ServiceRequests.FindAsync(assignment.RequestId);
                if (serviceRequest != null)
                {
                    var oldStatus = serviceRequest.CurrentStatus;
                    serviceRequest.CurrentStatus = "Assigned";

                    if (!string.Equals(oldStatus, serviceRequest.CurrentStatus, StringComparison.OrdinalIgnoreCase))
                    {
                        _context.StatusHistories.Add(new StatusHistory
                        {
                            RequestId = serviceRequest.RequestId,
                            OldStatus = oldStatus,
                            NewStatus = serviceRequest.CurrentStatus,
                            ChangedBy = assignment.AssignedBy,
                            ChangedAt = DateTime.Now
                        });
                    }
                }

                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }

            ViewData["AssignedBy"] = new SelectList(
                _context.Users.Where(u => u.UserId == userId),
                "UserId",
                "Name",
                userId);

            ViewData["RequestId"] = new SelectList(_context.ServiceRequests, "RequestId", "TrackingNumber", assignment.RequestId);
            ViewData["TechnicianId"] = new SelectList(_context.Users, "UserId", "Name", assignment.TechnicianId);

            return View(assignment);
        }

        // GET: Assignments/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (!IsLoggedIn())
            {
                return RedirectToAction("Login", "Account");
            }

            if (!HasRole("Manager", "Admin"))
            {
                return RedirectToAction("Index", "Home");
            }

            if (id == null)
            {
                return NotFound();
            }

            var assignment = await _context.Assignments.FindAsync(id);
            if (assignment == null)
            {
                return NotFound();
            }

            ViewData["AssignedBy"] = new SelectList(_context.Users, "UserId", "Name", assignment.AssignedBy);
            ViewData["RequestId"] = new SelectList(_context.ServiceRequests, "RequestId", "TrackingNumber", assignment.RequestId);
            ViewData["TechnicianId"] = new SelectList(_context.Users, "UserId", "Name", assignment.TechnicianId);

            return View(assignment);
        }

        // POST: Assignments/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("AssignmentId,RequestId,TechnicianId,AssignedBy,AssignedAt")] Assignment assignment)
        {
            var currentUserId = GetCurrentUserId();

            if (!IsLoggedIn() || !currentUserId.HasValue)
            {
                return RedirectToAction("Login", "Account");
            }

            if (!HasRole("Manager", "Admin"))
            {
                return RedirectToAction("Index", "Home");
            }

            if (id != assignment.AssignmentId)
            {
                return NotFound();
            }

            ModelState.Remove(nameof(Assignment.Request));
            ModelState.Remove(nameof(Assignment.Technician));
            ModelState.Remove(nameof(Assignment.AssignedByNavigation));

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(assignment);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!AssignmentExists(assignment.AssignmentId))
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

            ViewData["AssignedBy"] = new SelectList(_context.Users, "UserId", "Name", assignment.AssignedBy);
            ViewData["RequestId"] = new SelectList(_context.ServiceRequests, "RequestId", "TrackingNumber", assignment.RequestId);
            ViewData["TechnicianId"] = new SelectList(_context.Users, "UserId", "Name", assignment.TechnicianId);

            return View(assignment);
        }

        // GET: Assignments/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (!IsLoggedIn())
            {
                return RedirectToAction("Login", "Account");
            }

            if (!HasRole("Manager", "Admin"))
            {
                return RedirectToAction("Index", "Home");
            }

            if (id == null)
            {
                return NotFound();
            }

            var assignment = await _context.Assignments
                .Include(a => a.AssignedByNavigation)
                .Include(a => a.Request)
                .Include(a => a.Technician)
                .FirstOrDefaultAsync(m => m.AssignmentId == id);

            if (assignment == null)
            {
                return NotFound();
            }

            return View(assignment);
        }

        // POST: Assignments/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            if (!IsLoggedIn())
            {
                return RedirectToAction("Login", "Account");
            }

            if (!HasRole("Manager", "Admin"))
            {
                return RedirectToAction("Index", "Home");
            }

            var assignment = await _context.Assignments.FindAsync(id);
            if (assignment != null)
            {
                _context.Assignments.Remove(assignment);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool AssignmentExists(int id)
        {
            return _context.Assignments.Any(e => e.AssignmentId == id);
        }
    }
}