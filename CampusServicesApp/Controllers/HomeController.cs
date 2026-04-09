using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using CampusServicesApp.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Http;
using System.Text;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

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

    public IActionResult Index() => View();

    public IActionResult Privacy() => View();

    public async Task<IActionResult> ReportingDashboard()
    {
        if (!IsLoggedIn())
            return RedirectToAction("Login", "Account");

        if (!HasRole("Admin", "Manager"))
            return RedirectToAction("Index", "Home");

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

    // 🔹 Shared Filter Logic
    private IQueryable<ServiceRequest> BuildFilteredRequestsQuery(string? status, string? categoryName, DateTime? startDate, DateTime? endDate)
    {
        var query = _context.ServiceRequests
            .Include(r => r.Category)
            .Include(r => r.Requester)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(status))
            query = query.Where(r => r.CurrentStatus == status);

        if (!string.IsNullOrWhiteSpace(categoryName))
            query = query.Where(r => r.Category != null && r.Category.CategoryName == categoryName);

        if (startDate.HasValue)
            query = query.Where(r => r.CreatedAt >= startDate.Value);

        if (endDate.HasValue)
        {
            var inclusiveEndDate = endDate.Value.Date.AddDays(1);
            query = query.Where(r => r.CreatedAt < inclusiveEndDate);
        }

        return query.OrderBy(r => r.CreatedAt);
    }

    // 🔹 PDF EXPORT
    public async Task<IActionResult> ExportPdf(string? status, string? categoryName, DateTime? startDate, DateTime? endDate)
    {
        if (!IsLoggedIn())
            return RedirectToAction("Login", "Account");

        if (!HasRole("Admin", "Manager"))
            return RedirectToAction("Index", "Home");

        var requests = await BuildFilteredRequestsQuery(status, categoryName, startDate, endDate).ToListAsync();

        QuestPDF.Settings.License = LicenseType.Community;

        var pdfBytes = Document.Create(container =>
        {
            container.Page(page =>
            {
                page.Margin(20);
                page.Size(PageSizes.A4.Landscape());

                page.Header().Text("Service Requests Report").Bold().FontSize(18);

                page.Content().Table(table =>
                {
                    table.ColumnsDefinition(columns =>
                    {
                        columns.RelativeColumn();
                        columns.RelativeColumn();
                        columns.RelativeColumn();
                        columns.RelativeColumn();
                        columns.RelativeColumn();
                        columns.RelativeColumn();
                    });

                    table.Header(header =>
                    {
                        header.Cell().Text("Tracking #").Bold();
                        header.Cell().Text("Status").Bold();
                        header.Cell().Text("Category").Bold();
                        header.Cell().Text("Requester").Bold();
                        header.Cell().Text("Created").Bold();
                        header.Cell().Text("Closed").Bold();
                    });

                    foreach (var r in requests)
                    {
                        table.Cell().Text(r.TrackingNumber);
                        table.Cell().Text(r.CurrentStatus);
                        table.Cell().Text(r.Category?.CategoryName ?? "");
                        table.Cell().Text(r.Requester?.Name ?? "");
                        table.Cell().Text(r.CreatedAt.ToString("g"));
                        table.Cell().Text(r.ClosedAt?.ToString("g") ?? "");
                    }
                });
            });
        }).GeneratePdf();

        return File(pdfBytes, "application/pdf", "report.pdf");
    }

    // 🔹 CSV EXPORT (UPDATED)
    public async Task<IActionResult> ExportCsv(string? status, string? categoryName, DateTime? startDate, DateTime? endDate)
    {
        if (!IsLoggedIn())
            return RedirectToAction("Login", "Account");

        if (!HasRole("Admin", "Manager"))
            return RedirectToAction("Index", "Home");

        var requests = await BuildFilteredRequestsQuery(status, categoryName, startDate, endDate).ToListAsync();

        var sb = new StringBuilder();
        sb.AppendLine("TrackingNumber,Status,Category,Requester,CreatedAt,ClosedAt");

        foreach (var request in requests)
        {
            sb.AppendLine($"{request.TrackingNumber},{request.CurrentStatus},{request.Category?.CategoryName},{request.Requester?.Name},{request.CreatedAt:g},{request.ClosedAt:g}");
        }

        return File(Encoding.UTF8.GetBytes(sb.ToString()), "text/csv", "report.csv");
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}