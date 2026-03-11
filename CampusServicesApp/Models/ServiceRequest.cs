using System;
using System.Collections.Generic;

namespace CampusServicesApp.Models;

public partial class ServiceRequest
{
    public int RequestId { get; set; }

    public string TrackingNumber { get; set; } = null!;

    public int RequesterId { get; set; }

    public int CategoryId { get; set; }

    public int TeamId { get; set; }

    public string CurrentStatus { get; set; } = null!;

    public string Description { get; set; } = null!;

    public DateTime CreatedAt { get; set; }

    public DateTime? ClosedAt { get; set; }

    public virtual ICollection<Assignment> Assignments { get; set; } = new List<Assignment>();

    public virtual Category Category { get; set; } = null!;

    public virtual ICollection<Note> Notes { get; set; } = new List<Note>();

    public virtual ICollection<Notification> Notifications { get; set; } = new List<Notification>();

    public virtual User Requester { get; set; } = null!;

    public virtual ICollection<StatusHistory> StatusHistories { get; set; } = new List<StatusHistory>();

    public virtual ServiceTeam Team { get; set; } = null!;
}
