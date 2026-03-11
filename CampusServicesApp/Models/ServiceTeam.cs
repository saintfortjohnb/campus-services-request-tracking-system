using System;
using System.Collections.Generic;

namespace CampusServicesApp.Models;

public partial class ServiceTeam
{
    public int TeamId { get; set; }

    public string TeamName { get; set; } = null!;

    public string? TeamEmail { get; set; }

    public bool IsActive { get; set; }

    public virtual ICollection<Category> Categories { get; set; } = new List<Category>();

    public virtual ICollection<ServiceRequest> ServiceRequests { get; set; } = new List<ServiceRequest>();
}
