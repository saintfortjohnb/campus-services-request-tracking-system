using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CampusServicesApp.Models
{
    public class Note
    {
        [Key]
        public int NoteId { get; set; }

        [Required]
        public int RequestId { get; set; }

        [Required]
        public int AuthorId { get; set; }

        [Required]
        public string NoteText { get; set; } = string.Empty;

        public DateTime CreatedAt { get; set; }

        [ForeignKey(nameof(RequestId))]
        public virtual ServiceRequest? Request { get; set; }

        [ForeignKey(nameof(AuthorId))]
        public virtual User? Author { get; set; }
    }
}