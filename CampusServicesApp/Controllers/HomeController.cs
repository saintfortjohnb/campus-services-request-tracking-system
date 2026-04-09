using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using CampusServicesApp.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Http;
using System.Text;

namespace CampusServicesApp.Controllers;

public class HomeController : Controller
{
    private readonly ApplicationDbContext _context;

    public HomeController(ApplicationDbContext context)
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

    public IActionResult Index()
    {
        return View();
    }

    public IActionResult Privacy()
    {
        return View();
    }

    public async Task<IActionResult> ReportingDashboard()
    {
        if (!IsLoggedIn())
        {
            return RedirectToAction("Login", "Account");
        }

        if (!HasRole("Admin", "Manager"))
        {
            return RedirectToAction("Index", "Home");
        }

        var model = new ReportingDashboardViewModel
        {
            OpenCount = await _context.ServiceRequests.CountAsync(r =>
                r.CurrentStatus != "Resolved" && r.CurrentStatus != "Closed"),

            ResolvedCount = await _context.ServiceRequests.CountAsync(r =>
                r.CurrentStatus == "Resolved"),

            ClosedCount = await _context.ServiceRequests.CountAsync(r =>
                r.CurrentStatus == "Closed"),

            RequestsByCategory = await _context.ServiceRequests
                .Include(r => r.Category)
                .GroupBy(r => r.Category.CategoryName)
                .Select(g => new { Category = g.Key, Count = g.Count() })
                .ToDictionaryAsync(x => x.Category, x => x.Count),

            RequestsByStatus = await _context.ServiceRequests
                .GroupBy(r => r.CurrentStatus)
                .Select(g => new { Status = g.Key, Count = g.Count() })
                .ToDictionaryAsync(x => x.Status, x => x.Count)
        };

        return View(model);
    }

    public async Task<IActionResult> ExportCsv(string? status, string? categoryName, DateTime? startDate, DateTime? endDate)
    {
        if (!IsLoggedIn())
        {
            return RedirectToAction("Login", "Account");
        }

        if (!HasRole("Admin", "Manager"))
        {
            return RedirectToAction("Index", "Home");
        }

        var query = _context.ServiceRequests
            .Include(r => r.Category)
            .Include(r => r.Requester)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(status))
        {
            query = query.Where(r => r.CurrentStatus == status);
        }

        if (!string.IsNullOrWhiteSpace(categoryName))
        {
            query = query.Where(r => r.Category != null && r.Category.CategoryName == categoryName);
        }

        if (startDate.HasValue)
        {
            query = query.Where(r => r.CreatedAt >= startDate.Value);
        }

        if (endDate.HasValue)
        {
            var inclusiveEndDate = endDate.Value.Date.AddDays(1);
            query = query.Where(r => r.CreatedAt < inclusiveEndDate);
        }

        var requests = await query
            .OrderBy(r => r.CreatedAt)
            .ToListAsync();

        var sb = new StringBuilder();
        sb.AppendLine("TrackingNumber,Status,Category,Requester,CreatedAt,ClosedAt");

        foreach (var request in requests)
        {
            var trackingNumber = EscapeCsv(request.TrackingNumber);
            var currentStatus = EscapeCsv(request.CurrentStatus);
            var category = EscapeCsv(request.Category?.CategoryName ?? string.Empty);
            var requester = EscapeCsv(request.Requester?.Name ?? string.Empty);
            var createdAt = request.CreatedAt.ToString("g");
            var closedAt = request.ClosedAt?.ToString("g") ?? string.Empty;

            sb.AppendLine($"{trackingNumber},{currentStatus},{category},{requester},{createdAt},{closedAt}");
        }

        var bytes = Encoding.UTF8.GetBytes(sb.ToString());
        return File(bytes, "text/csv", "service-requests-report-filtered.csv");
    }

    private static string EscapeCsv(string value)
    {
        if (string.IsNullOrEmpty(value))
        {
            return string.Empty;
        }

        if (value.Contains(",") || value.Contains("\"") || value.Contains("\n"))
        {
            return $"\"{value.Replace("\"", "\"\"")}\"";
        }

        return value;
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}
