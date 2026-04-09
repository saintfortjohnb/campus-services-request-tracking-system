using System.Collections.Generic;

namespace CampusServicesApp.Models
{
    public class ReportingDashboardViewModel
    {
        public int OpenCount { get; set; }
        public int ResolvedCount { get; set; }
        public int ClosedCount { get; set; }

        public Dictionary<string, int> RequestsByCategory { get; set; } = new();
        public Dictionary<string, int> RequestsByStatus { get; set; } = new();
    }
}