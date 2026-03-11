using System;
using System.Collections.Generic;

namespace CampusServicesApp.Models;

public partial class Notification
{
    public int NotificationId { get; set; }

    public int RequestId { get; set; }

    public int RecipientId { get; set; }

    public string Message { get; set; } = null!;

    public string Channel { get; set; } = null!;

    public DateTime? SentAt { get; set; }

    public string DeliveryStatus { get; set; } = null!;

    public virtual User Recipient { get; set; } = null!;

    public virtual ServiceRequest Request { get; set; } = null!;
}
