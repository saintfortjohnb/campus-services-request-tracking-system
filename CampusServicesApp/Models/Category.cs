using System;
using System.Collections.Generic;

namespace CampusServicesApp.Models;

public partial class Category
{
    public int CategoryId { get; set; }

    public string CategoryName { get; set; } = null!;

    public int DefaultTeamId { get; set; }

    public bool IsActive { get; set; }

    public virtual ServiceTeam DefaultTeam { get; set; } = null!;

    public virtual ICollection<ServiceRequest> ServiceRequests { get; set; } = new List<ServiceRequest>();
}
