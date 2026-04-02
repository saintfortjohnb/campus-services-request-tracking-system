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
    public class AssignmentsController : Controller
    {
        private readonly ApplicationDbContext _context;

        public AssignmentsController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: Assignments
        public async Task<IActionResult> Index()
        {
            var applicationDbContext = _context.Assignments.Include(a => a.AssignedByNavigation).Include(a => a.Request).Include(a => a.Technician);
            return View(await applicationDbContext.ToListAsync());
        }

        // GET: Assignments/Details/5
        public async Task<IActionResult> Details(int? id)
        {
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

        // GET: Assignments/Create
        public IActionResult Create()
        {
            ViewData["AssignedBy"] = new SelectList(_context.Users, "UserId", "Name");
            ViewData["RequestId"] = new SelectList(_context.ServiceRequests, "RequestId", "TrackingNumber");
            ViewData["TechnicianId"] = new SelectList(_context.Users, "UserId", "Name");
            return View();
        }

        // POST: Assignments/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("RequestId,TechnicianId,AssignedBy,AssignedAt")] Assignment assignment)
        {
            
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
                    serviceRequest.CurrentStatus = "Assigned";
                }

                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            ViewData["AssignedBy"] = new SelectList(_context.Users, "UserId", "Name", assignment.AssignedBy);
            ViewData["RequestId"] = new SelectList(_context.ServiceRequests, "RequestId", "TrackingNumber", assignment.RequestId);
            ViewData["TechnicianId"] = new SelectList(_context.Users, "UserId", "Name", assignment.TechnicianId);
            return View(assignment);
        }

        // GET: Assignments/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
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
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("AssignmentId,RequestId,TechnicianId,AssignedBy,AssignedAt")] Assignment assignment)
        {
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
