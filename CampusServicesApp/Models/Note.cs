using System;
using System.Collections.Generic;

namespace CampusServicesApp.Models;

public partial class Note
{
    public int NoteId { get; set; }

    public int RequestId { get; set; }

    public int AuthorId { get; set; }

    public string NoteText { get; set; } = null!;

    public DateTime CreatedAt { get; set; }

    public virtual User Author { get; set; } = null!;

    public virtual ServiceRequest Request { get; set; } = null!;
}
