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
    public class ServiceRequestsController : Controller
    {
        private readonly ApplicationDbContext _context;

        public ServiceRequestsController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: ServiceRequests
        public async Task<IActionResult> Index()
        {
            var applicationDbContext = _context.ServiceRequests.Include(s => s.Category).Include(s => s.Requester).Include(s => s.Team);
            return View(await applicationDbContext.ToListAsync());
        }

        // GET: ServiceRequests/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var serviceRequest = await _context.ServiceRequests
                .Include(s => s.Category)
                .Include(s => s.Requester)
                .Include(s => s.Team)
                .FirstOrDefaultAsync(m => m.RequestId == id);
            if (serviceRequest == null)
            {
                return NotFound();
            }

            return View(serviceRequest);
        }

        // GET: ServiceRequests/Create
        public IActionResult Create()
        {
            PopulateDropDowns();
            return View();
        }

        // POST: ServiceRequests/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("TrackingNumber,RequesterId,CategoryId,TeamId,CurrentStatus,Description")] ServiceRequest serviceRequest)
        {
            serviceRequest.CreatedAt = DateTime.Now;
            serviceRequest.ClosedAt = null;

            if (string.IsNullOrWhiteSpace(serviceRequest.CurrentStatus))
            {
                serviceRequest.CurrentStatus = "Open";
            }

            ModelState.Remove(nameof(ServiceRequest.CreatedAt));
            ModelState.Remove(nameof(ServiceRequest.ClosedAt));
            ModelState.Remove(nameof(ServiceRequest.Category));
            ModelState.Remove(nameof(ServiceRequest.Requester));
            ModelState.Remove(nameof(ServiceRequest.Team));

            if (ModelState.IsValid)
            {
                _context.Add(serviceRequest);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            
            foreach (var entry in ModelState)
            {
                foreach (var error in entry.Value.Errors)
                {
                    ModelState.AddModelError(string.Empty, $"{entry.Key}: {error.ErrorMessage}");
                }
            }

            PopulateDropDowns(serviceRequest.CategoryId, serviceRequest.RequesterId, serviceRequest.TeamId);
            return View(serviceRequest);
        }

        // GET: ServiceRequests/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var serviceRequest = await _context.ServiceRequests.FindAsync(id);
            if (serviceRequest == null)
            {
                return NotFound();
            }
            PopulateDropDowns(serviceRequest.CategoryId, serviceRequest.RequesterId, serviceRequest.TeamId);
            return View(serviceRequest);
        }

        // POST: ServiceRequests/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("RequestId,TrackingNumber,RequesterId,CategoryId,TeamId,CurrentStatus,Description")] ServiceRequest serviceRequest)
        {
            if (id != serviceRequest.RequestId)
            {
                return NotFound();
            }

            ModelState.Remove(nameof(ServiceRequest.CreatedAt));
            ModelState.Remove(nameof(ServiceRequest.ClosedAt));
            ModelState.Remove(nameof(ServiceRequest.Category));
            ModelState.Remove(nameof(ServiceRequest.Requester));
            ModelState.Remove(nameof(ServiceRequest.Team));

            if (ModelState.IsValid)
            {
                var existingRequest = await _context.ServiceRequests.FindAsync(id);
                if (existingRequest == null)
                {
                    return NotFound();
                }

                existingRequest.TrackingNumber = serviceRequest.TrackingNumber;
                existingRequest.RequesterId = serviceRequest.RequesterId;
                existingRequest.CategoryId = serviceRequest.CategoryId;
                existingRequest.TeamId = serviceRequest.TeamId;
                existingRequest.CurrentStatus = serviceRequest.CurrentStatus;
                existingRequest.Description = serviceRequest.Description;

                if (string.Equals(existingRequest.CurrentStatus, "Closed", StringComparison.OrdinalIgnoreCase) && existingRequest.ClosedAt == null)
                {
                    existingRequest.ClosedAt = DateTime.Now;
                }
                else if (!string.Equals(existingRequest.CurrentStatus, "Closed", StringComparison.OrdinalIgnoreCase))
                {
                    existingRequest.ClosedAt = null;
                }

                try
                {
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!ServiceRequestExists(serviceRequest.RequestId))
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

            foreach (var entry in ModelState)
            {
                foreach (var error in entry.Value.Errors)
                {
                    ModelState.AddModelError(string.Empty, $"{entry.Key}: {error.ErrorMessage}");
                }
            }

            PopulateDropDowns(serviceRequest.CategoryId, serviceRequest.RequesterId, serviceRequest.TeamId);
            return View(serviceRequest);
        }

        // GET: ServiceRequests/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var serviceRequest = await _context.ServiceRequests
                .Include(s => s.Category)
                .Include(s => s.Requester)
                .Include(s => s.Team)
                .FirstOrDefaultAsync(m => m.RequestId == id);
            if (serviceRequest == null)
            {
                return NotFound();
            }

            return View(serviceRequest);
        }

        // POST: ServiceRequests/Delete/5
                [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var serviceRequest = await _context.ServiceRequests.FindAsync(id);
            if (serviceRequest == null)
            {
                return RedirectToAction(nameof(Index));
            }

            var relatedAssignments = await _context.Assignments
                .Where(a => a.RequestId == id)
                .ToListAsync();

            if (relatedAssignments.Any())
            {
                _context.Assignments.RemoveRange(relatedAssignments);
            }

            var relatedStatusHistory = await _context.StatusHistories
                .Where(s => s.RequestId == id)
                .ToListAsync();

            if (relatedStatusHistory.Any())
            {
                _context.StatusHistories.RemoveRange(relatedStatusHistory);
            }

            _context.ServiceRequests.Remove(serviceRequest);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private void PopulateDropDowns(int? categoryId = null, int? requesterId = null, int? teamId = null)
        {
            var categoryItems = _context.Categories
                .AsNoTracking()
                .ToList()
                .Select(c => new
                {
                    Value = c.CategoryId,
                    Text = GetDisplayValue(c, "CategoryName", "Name", "Category") ?? $"Category {c.CategoryId}"
                });

            var requesterItems = _context.Users
                .AsNoTracking()
                .ToList()
                .Select(u => new
                {
                    Value = u.UserId,
                    Text = GetDisplayValue(u, "Name", "FullName", "DisplayName", "Username", "Email") ?? $"User {u.UserId}"
                });

            var teamItems = _context.ServiceTeams
                .AsNoTracking()
                .ToList()
                .Select(t => new
                {
                    Value = t.TeamId,
                    Text = GetDisplayValue(t, "TeamName", "Name", "TeamCode") ?? $"Team {t.TeamId}"
                });

            ViewData["CategoryId"] = new SelectList(categoryItems, "Value", "Text", categoryId);
            ViewData["RequesterId"] = new SelectList(requesterItems, "Value", "Text", requesterId);
            ViewData["TeamId"] = new SelectList(teamItems, "Value", "Text", teamId);
        }

        private static string? GetDisplayValue(object entity, params string[] propertyNames)
        {
            foreach (var propertyName in propertyNames)
            {
                var property = entity.GetType().GetProperty(propertyName);
                if (property != null)
                {
                    var value = property.GetValue(entity)?.ToString();
                    if (!string.IsNullOrWhiteSpace(value))
                    {
                        return value;
                    }
                }
            }

            return null;
        }

        private bool ServiceRequestExists(int id)
        {
            return _context.ServiceRequests.Any(e => e.RequestId == id);
        }
    }
}
