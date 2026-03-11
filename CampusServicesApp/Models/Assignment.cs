using System;
using System.Collections.Generic;

namespace CampusServicesApp.Models;

public partial class Assignment
{
    public int AssignmentId { get; set; }

    public int RequestId { get; set; }

    public int TechnicianId { get; set; }

    public int AssignedBy { get; set; }

    public DateTime AssignedAt { get; set; }

    public virtual User AssignedByNavigation { get; set; } = null!;

    public virtual ServiceRequest Request { get; set; } = null!;

    public virtual User Technician { get; set; } = null!;
}
