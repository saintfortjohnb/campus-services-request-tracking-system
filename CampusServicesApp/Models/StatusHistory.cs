using System;
using System.Collections.Generic;

namespace CampusServicesApp.Models;

public partial class StatusHistory
{
    public int StatusHistoryId { get; set; }

    public int RequestId { get; set; }

    public string? OldStatus { get; set; }

    public string NewStatus { get; set; } = null!;

    public int ChangedBy { get; set; }

    public DateTime ChangedAt { get; set; }

    public virtual User ChangedByNavigation { get; set; } = null!;

    public virtual ServiceRequest Request { get; set; } = null!;
}
