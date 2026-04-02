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
            var openRequests = await _context.ServiceRequests
                .Include(s => s.Category)
                .Include(s => s.Requester)
                .Include(s => s.Team)
                .Where(s => s.CurrentStatus != "Resolved" && s.CurrentStatus != "Closed")
                .ToListAsync();

            ViewData["Title"] = "Open Requests";
            return View(openRequests);
        }

        // GET: ServiceRequests/Resolved
        public async Task<IActionResult> Resolved()
        {
            var resolvedRequests = await _context.ServiceRequests
                .Include(s => s.Category)
                .Include(s => s.Requester)
                .Include(s => s.Team)
                .Where(s => s.CurrentStatus == "Resolved")
                .ToListAsync();

            ViewData["Title"] = "Resolved Requests";
            return View("Index", resolvedRequests);
        }

        // GET: ServiceRequests/Closed
        public async Task<IActionResult> Closed()
        {
            var closedRequests = await _context.ServiceRequests
                .Include(s => s.Category)
                .Include(s => s.Requester)
                .Include(s => s.Team)
                .Where(s => s.CurrentStatus == "Closed")
                .ToListAsync();

            ViewData["Title"] = "Closed Requests";
            return View("Index", closedRequests);
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
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("RequesterId,CategoryId,Description")] ServiceRequest serviceRequest)
        {
            serviceRequest.CreatedAt = DateTime.Now;
            serviceRequest.ClosedAt = null;

            var selectedCategory = await _context.Categories
                .AsNoTracking()
                .FirstOrDefaultAsync(c => c.CategoryId == serviceRequest.CategoryId);

            if (selectedCategory == null)
            {
                ModelState.AddModelError(nameof(ServiceRequest.CategoryId), "Please select a valid category.");
            }
            else
            {
                serviceRequest.TeamId = selectedCategory.DefaultTeamId;
            }

            var currentYear = DateTime.Now.Year;
            var yearSuffix = currentYear % 100;

            var countForYear = await _context.ServiceRequests
                .CountAsync(r => r.CreatedAt.Year == currentYear);

            var nextNumber = countForYear + 1;
            serviceRequest.TrackingNumber = $"REQ-{yearSuffix}-{nextNumber:D3}";

            serviceRequest.CurrentStatus = "Submitted";

            ModelState.Remove(nameof(ServiceRequest.CreatedAt));
            ModelState.Remove(nameof(ServiceRequest.ClosedAt));
            ModelState.Remove(nameof(ServiceRequest.TrackingNumber));
            ModelState.Remove(nameof(ServiceRequest.CurrentStatus));
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

            PopulateDropDowns(serviceRequest.CategoryId, serviceRequest.RequesterId);
            return View(serviceRequest);
        }

        // GET: ServiceRequests/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var serviceRequest = await _context.ServiceRequests
                .Include(s => s.Requester)
                .FirstOrDefaultAsync(s => s.RequestId == id);

            if (serviceRequest == null)
            {
                return NotFound();
            }

            PopulateDropDowns(serviceRequest.CategoryId, serviceRequest.RequesterId, serviceRequest.TeamId);
            return View(serviceRequest);
        }

        // POST: ServiceRequests/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("RequestId,CategoryId,CurrentStatus,Description")] ServiceRequest serviceRequest)
        {
            if (id != serviceRequest.RequestId)
            {
                return NotFound();
            }

            ModelState.Remove(nameof(ServiceRequest.CreatedAt));
            ModelState.Remove(nameof(ServiceRequest.ClosedAt));
            ModelState.Remove(nameof(ServiceRequest.TrackingNumber));
            ModelState.Remove(nameof(ServiceRequest.Category));
            ModelState.Remove(nameof(ServiceRequest.Requester));
            ModelState.Remove(nameof(ServiceRequest.Team));

            var existingRequest = await _context.ServiceRequests.FindAsync(id);
            if (existingRequest == null)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                var selectedCategory = await _context.Categories
                    .AsNoTracking()
                    .FirstOrDefaultAsync(c => c.CategoryId == serviceRequest.CategoryId);

                if (selectedCategory == null)
                {
                    ModelState.AddModelError(nameof(ServiceRequest.CategoryId), "Please select a valid category.");
                }
                else
                {
                    existingRequest.CategoryId = serviceRequest.CategoryId;
                    existingRequest.TeamId = selectedCategory.DefaultTeamId;
                    existingRequest.CurrentStatus = serviceRequest.CurrentStatus;
                    existingRequest.Description = serviceRequest.Description;
                }

                if (ModelState.IsValid)
                {
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
            }

            foreach (var entry in ModelState)
            {
                foreach (var error in entry.Value.Errors)
                {
                    ModelState.AddModelError(string.Empty, $"{entry.Key}: {error.ErrorMessage}");
                }
            }

            serviceRequest.RequesterId = existingRequest.RequesterId;
            serviceRequest.TeamId = existingRequest.TeamId;
            serviceRequest.TrackingNumber = existingRequest.TrackingNumber;
            serviceRequest.CreatedAt = existingRequest.CreatedAt;
            serviceRequest.ClosedAt = existingRequest.ClosedAt;
            
            var requester = await _context.Users.FindAsync(existingRequest.RequesterId);
            if (requester != null)
            {
                serviceRequest.Requester = requester;
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