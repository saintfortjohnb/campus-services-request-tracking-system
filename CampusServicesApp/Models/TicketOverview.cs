using System;
using System.Collections.Generic;

namespace CampusServicesApp.Models;

public partial class TicketOverview
{
    public string TrackingNumber { get; set; } = null!;

    public string? Requester { get; set; }

    public string? Technician { get; set; }

    public string? CategoryName { get; set; }

    public string? TeamName { get; set; }

    public string CurrentStatus { get; set; } = null!;

    public string? NoteText { get; set; }

    public string? Message { get; set; }
}
