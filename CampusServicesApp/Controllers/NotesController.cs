using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using CampusServicesApp.Models;

namespace CampusServicesApp.Controllers
{
    public class NotesController : Controller
    {
        private readonly ApplicationDbContext _context;

        public NotesController(ApplicationDbContext context)
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

        // GET: Notes
        public async Task<IActionResult> Index()
        {
            var currentUserId = GetCurrentUserId();
            if (!IsLoggedIn() || !currentUserId.HasValue)
            {
                return RedirectToAction("Login", "Account");
            }

            var userId = currentUserId.Value;

            var query = _context.Notes
                .Include(n => n.Request)
                .Include(n => n.Author)
                .AsQueryable();

            if (HasRole("Requester") && !HasRole("Manager", "Admin", "Technician"))
            {
                query = query.Where(n => n.Request != null && n.Request.RequesterId == userId);
            }

            var notes = await query
                .OrderByDescending(n => n.CreatedAt)
                .ToListAsync();

            return View(notes);
        }

        // GET: Notes/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            var currentUserId = GetCurrentUserId();
            if (!IsLoggedIn() || !currentUserId.HasValue)
            {
                return RedirectToAction("Login", "Account");
            }

            var userId = currentUserId.Value;

            if (id == null)
            {
                return NotFound();
            }

            var note = await _context.Notes
                .Include(n => n.Request)
                .Include(n => n.Author)
                .FirstOrDefaultAsync(m => m.NoteId == id);

            if (note == null)
            {
                return NotFound();
            }

            var canAccess = HasRole("Admin", "Manager", "Technician") ||
                            (note.Request != null && note.Request.RequesterId == userId);
            if (!canAccess)
            {
                return RedirectToAction("Index", "Home");
            }

            return View(note);
        }

        // GET: Notes/Create
        public IActionResult Create(int? requestId)
        {
            var currentUserId = GetCurrentUserId();
            if (!IsLoggedIn() || !currentUserId.HasValue)
            {
                return RedirectToAction("Login", "Account");
            }

            var userId = currentUserId.Value;

            if (requestId != null)
            {
                var request = _context.ServiceRequests
                    .FirstOrDefault(r => r.RequestId == requestId);

                if (request == null)
                {
                    return NotFound();
                }

                var isAdmin = HasRole("Admin");
                var isManager = HasRole("Manager");
                var isTechnician = HasRole("Technician");
                var isRequesterForThisRequest = request.RequesterId == userId;

                if (!isAdmin && !isManager && !isTechnician && !isRequesterForThisRequest)
                {
                    return RedirectToAction("Details", "ServiceRequests", new { id = requestId.Value });
                }

                if (request != null)
                {
                    ViewBag.TrackingNumber = request.TrackingNumber;
                }

                ViewData["RequestId"] = new SelectList(
                    _context.ServiceRequests,
                    "RequestId",
                    "TrackingNumber",
                    requestId
                );

                return View(new Note { RequestId = requestId.Value });
            }

            ViewData["RequestId"] = new SelectList(_context.ServiceRequests, "RequestId", "TrackingNumber");
            return View(new Note());
        }

        // POST: Notes/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("RequestId,NoteText")] Note note)
        {
            var currentUserId = GetCurrentUserId();
            if (!IsLoggedIn() || !currentUserId.HasValue)
            {
                return RedirectToAction("Login", "Account");
            }

            var userId = currentUserId.Value;

            var request = await _context.ServiceRequests
                .FirstOrDefaultAsync(r => r.RequestId == note.RequestId);

            if (request == null)
            {
                return NotFound();
            }

            var isAdmin = HasRole("Admin");
            var isManager = HasRole("Manager");
            var isTechnician = HasRole("Technician");
            var isRequesterForThisRequest = request.RequesterId == userId;

            if (!isAdmin && !isManager && !isTechnician && !isRequesterForThisRequest)
            {
                return RedirectToAction("Details", "ServiceRequests", new { id = note.RequestId });
            }

            note.AuthorId = userId;
            note.CreatedAt = DateTime.Now;

            ModelState.Remove(nameof(Note.Author));
            ModelState.Remove(nameof(Note.Request));

            if (ModelState.IsValid)
            {
                _context.Add(note);
                await _context.SaveChangesAsync();
                return RedirectToAction("Details", "ServiceRequests", new { id = note.RequestId });
            }

            ViewData["RequestId"] = new SelectList(_context.ServiceRequests, "RequestId", "TrackingNumber", note.RequestId);
            ViewBag.TrackingNumber = request.TrackingNumber;

            return View(note);
        }

        // GET: Notes/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            var currentUserId = GetCurrentUserId();
            if (!IsLoggedIn() || !currentUserId.HasValue)
            {
                return RedirectToAction("Login", "Account");
            }

            var userId = currentUserId.Value;

            if (id == null)
            {
                return NotFound();
            }

            var note = await _context.Notes
                .Include(n => n.Request)
                .Include(n => n.Author)
                .FirstOrDefaultAsync(m => m.NoteId == id);

            if (note == null)
            {
                return NotFound();
            }

            var canDelete = HasRole("Admin", "Manager") || note.AuthorId == userId;
            if (!canDelete)
            {
                return RedirectToAction("Details", "ServiceRequests", new { id = note.RequestId });
            }

            return View(note);
        }

        // POST: Notes/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var currentUserId = GetCurrentUserId();
            if (!IsLoggedIn() || !currentUserId.HasValue)
            {
                return RedirectToAction("Login", "Account");
            }

            var userId = currentUserId.Value;

            var note = await _context.Notes
                .Include(n => n.Request)
                .FirstOrDefaultAsync(n => n.NoteId == id);

            if (note == null)
            {
                return RedirectToAction(nameof(Index));
            }

            var canDelete = HasRole("Admin", "Manager") || note.AuthorId == userId;
            if (!canDelete)
            {
                return RedirectToAction("Details", "ServiceRequests", new { id = note.RequestId });
            }

            var requestId = note.RequestId;
            _context.Notes.Remove(note);
            await _context.SaveChangesAsync();
            return RedirectToAction("Details", "ServiceRequests", new { id = requestId });
        }
    }
}