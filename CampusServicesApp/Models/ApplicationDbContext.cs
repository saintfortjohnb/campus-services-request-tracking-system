using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;

namespace CampusServicesApp.Models;

public partial class ApplicationDbContext : DbContext
{
    public ApplicationDbContext()
    {
    }

    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
    }

    public virtual DbSet<Assignment> Assignments { get; set; }

    public virtual DbSet<Category> Categories { get; set; }

    public virtual DbSet<Note> Notes { get; set; }

    public virtual DbSet<Notification> Notifications { get; set; }

    public virtual DbSet<Role> Roles { get; set; }

    public virtual DbSet<ServiceRequest> ServiceRequests { get; set; }

    public virtual DbSet<ServiceTeam> ServiceTeams { get; set; }

    public virtual DbSet<StatusHistory> StatusHistories { get; set; }

    public virtual DbSet<TicketOverview> TicketOverviews { get; set; }

    public virtual DbSet<User> Users { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Assignment>(entity =>
        {
            entity.HasKey(e => e.AssignmentId).HasName("PK__Assignme__DA8918147BCE1C65");

            entity.Property(e => e.AssignmentId).HasColumnName("assignment_id");
            entity.Property(e => e.AssignedAt)
                .HasDefaultValueSql("(sysdatetime())")
                .HasColumnName("assigned_at");
            entity.Property(e => e.AssignedBy).HasColumnName("assigned_by");
            entity.Property(e => e.RequestId).HasColumnName("request_id");
            entity.Property(e => e.TechnicianId).HasColumnName("technician_id");

            entity.HasOne(d => d.AssignedByNavigation).WithMany(p => p.AssignmentAssignedByNavigations)
                .HasForeignKey(d => d.AssignedBy)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Assignments_AssignedBy");

            entity.HasOne(d => d.Request).WithMany(p => p.Assignments)
                .HasForeignKey(d => d.RequestId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Assignments_ServiceRequests");

            entity.HasOne(d => d.Technician).WithMany(p => p.AssignmentTechnicians)
                .HasForeignKey(d => d.TechnicianId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Assignments_Technician");
        });

        modelBuilder.Entity<Category>(entity =>
        {
            entity.HasKey(e => e.CategoryId).HasName("PK__Categori__D54EE9B42B7E19EA");

            entity.HasIndex(e => e.CategoryName, "UQ__Categori__5189E25556AFDEF2").IsUnique();

            entity.Property(e => e.CategoryId).HasColumnName("category_id");
            entity.Property(e => e.CategoryName)
                .HasMaxLength(100)
                .IsUnicode(false)
                .HasColumnName("category_name");
            entity.Property(e => e.DefaultTeamId).HasColumnName("default_team_id");
            entity.Property(e => e.IsActive)
                .HasDefaultValue(true)
                .HasColumnName("is_active");

            entity.HasOne(d => d.DefaultTeam).WithMany(p => p.Categories)
                .HasForeignKey(d => d.DefaultTeamId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Categories_ServiceTeams");
        });

        modelBuilder.Entity<Note>(entity =>
        {
            entity.HasKey(e => e.NoteId).HasName("PK__Notes__CEDD0FA44781CBB1");

            entity.Property(e => e.NoteId).HasColumnName("note_id");
            entity.Property(e => e.AuthorId).HasColumnName("author_id");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("(sysdatetime())")
                .HasColumnName("created_at");
            entity.Property(e => e.NoteText)
                .IsUnicode(false)
                .HasColumnName("note_text");
            entity.Property(e => e.RequestId).HasColumnName("request_id");

            entity.HasOne(d => d.Author).WithMany(p => p.Notes)
                .HasForeignKey(d => d.AuthorId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Notes_Users");

            entity.HasOne(d => d.Request).WithMany(p => p.Notes)
                .HasForeignKey(d => d.RequestId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Notes_ServiceRequests");
        });

        modelBuilder.Entity<Notification>(entity =>
        {
            entity.HasKey(e => e.NotificationId).HasName("PK__Notifica__E059842F30700A28");

            entity.Property(e => e.NotificationId).HasColumnName("notification_id");
            entity.Property(e => e.Channel)
                .HasMaxLength(30)
                .IsUnicode(false)
                .HasColumnName("channel");
            entity.Property(e => e.DeliveryStatus)
                .HasMaxLength(30)
                .IsUnicode(false)
                .HasDefaultValue("Pending")
                .HasColumnName("delivery_status");
            entity.Property(e => e.Message)
                .HasMaxLength(255)
                .IsUnicode(false)
                .HasColumnName("message");
            entity.Property(e => e.RecipientId).HasColumnName("recipient_id");
            entity.Property(e => e.RequestId).HasColumnName("request_id");
            entity.Property(e => e.SentAt).HasColumnName("sent_at");

            entity.HasOne(d => d.Recipient).WithMany(p => p.Notifications)
                .HasForeignKey(d => d.RecipientId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Notifications_Users");

            entity.HasOne(d => d.Request).WithMany(p => p.Notifications)
                .HasForeignKey(d => d.RequestId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Notifications_ServiceRequests");
        });

        modelBuilder.Entity<Role>(entity =>
        {
            entity.HasKey(e => e.RoleId).HasName("PK__Roles__760965CC7FEC4791");

            entity.HasIndex(e => e.RoleName, "UQ__Roles__783254B1ACD6B9CB").IsUnique();

            entity.Property(e => e.RoleId).HasColumnName("role_id");
            entity.Property(e => e.RoleName)
                .HasMaxLength(50)
                .IsUnicode(false)
                .HasColumnName("role_name");
        });

        modelBuilder.Entity<ServiceRequest>(entity =>
        {
            entity.HasKey(e => e.RequestId).HasName("PK__ServiceR__18D3B90F890FD997");

            entity.HasIndex(e => e.TrackingNumber, "UQ__ServiceR__B2C338B7C0C32EAB").IsUnique();

            entity.Property(e => e.RequestId).HasColumnName("request_id");
            entity.Property(e => e.CategoryId).HasColumnName("category_id");
            entity.Property(e => e.ClosedAt).HasColumnName("closed_at");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("(sysdatetime())")
                .HasColumnName("created_at");
            entity.Property(e => e.CurrentStatus)
                .HasMaxLength(30)
                .IsUnicode(false)
                .HasColumnName("current_status");
            entity.Property(e => e.Description)
                .IsUnicode(false)
                .HasColumnName("description");
            entity.Property(e => e.RequesterId).HasColumnName("requester_id");
            entity.Property(e => e.TeamId).HasColumnName("team_id");
            entity.Property(e => e.TrackingNumber)
                .HasMaxLength(50)
                .IsUnicode(false)
                .HasColumnName("tracking_number");

            entity.HasOne(d => d.Category).WithMany(p => p.ServiceRequests)
                .HasForeignKey(d => d.CategoryId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_ServiceRequests_Categories");

            entity.HasOne(d => d.Requester).WithMany(p => p.ServiceRequests)
                .HasForeignKey(d => d.RequesterId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_ServiceRequests_Users");

            entity.HasOne(d => d.Team).WithMany(p => p.ServiceRequests)
                .HasForeignKey(d => d.TeamId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_ServiceRequests_ServiceTeams");
        });

        modelBuilder.Entity<ServiceTeam>(entity =>
        {
            entity.HasKey(e => e.TeamId).HasName("PK__ServiceT__F82DEDBC58090E2C");

            entity.HasIndex(e => e.TeamName, "UQ__ServiceT__29E35E0C2EC5FBF4").IsUnique();

            entity.Property(e => e.TeamId).HasColumnName("team_id");
            entity.Property(e => e.IsActive)
                .HasDefaultValue(true)
                .HasColumnName("is_active");
            entity.Property(e => e.TeamEmail)
                .HasMaxLength(150)
                .IsUnicode(false)
                .HasColumnName("team_email");
            entity.Property(e => e.TeamName)
                .HasMaxLength(100)
                .IsUnicode(false)
                .HasColumnName("team_name");
        });

        modelBuilder.Entity<StatusHistory>(entity =>
        {
            entity.HasKey(e => e.StatusHistoryId).HasName("PK__StatusHi__D79B1A252E72593A");

            entity.ToTable("StatusHistory");

            entity.Property(e => e.StatusHistoryId).HasColumnName("status_history_id");
            entity.Property(e => e.ChangedAt)
                .HasDefaultValueSql("(sysdatetime())")
                .HasColumnName("changed_at");
            entity.Property(e => e.ChangedBy).HasColumnName("changed_by");
            entity.Property(e => e.NewStatus)
                .HasMaxLength(30)
                .IsUnicode(false)
                .HasColumnName("new_status");
            entity.Property(e => e.OldStatus)
                .HasMaxLength(30)
                .IsUnicode(false)
                .HasColumnName("old_status");
            entity.Property(e => e.RequestId).HasColumnName("request_id");

            entity.HasOne(d => d.ChangedByNavigation).WithMany(p => p.StatusHistories)
                .HasForeignKey(d => d.ChangedBy)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_StatusHistory_Users");

            entity.HasOne(d => d.Request).WithMany(p => p.StatusHistories)
                .HasForeignKey(d => d.RequestId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_StatusHistory_ServiceRequests");
        });

        modelBuilder.Entity<TicketOverview>(entity =>
        {
            entity
                .HasNoKey()
                .ToView("TicketOverview");

            entity.Property(e => e.CategoryName)
                .HasMaxLength(100)
                .IsUnicode(false)
                .HasColumnName("category_name");
            entity.Property(e => e.CurrentStatus)
                .HasMaxLength(30)
                .IsUnicode(false)
                .HasColumnName("current_status");
            entity.Property(e => e.Message)
                .HasMaxLength(255)
                .IsUnicode(false)
                .HasColumnName("message");
            entity.Property(e => e.NoteText)
                .IsUnicode(false)
                .HasColumnName("note_text");
            entity.Property(e => e.Requester)
                .HasMaxLength(100)
                .IsUnicode(false)
                .HasColumnName("requester");
            entity.Property(e => e.TeamName)
                .HasMaxLength(100)
                .IsUnicode(false)
                .HasColumnName("team_name");
            entity.Property(e => e.Technician)
                .HasMaxLength(100)
                .IsUnicode(false)
                .HasColumnName("technician");
            entity.Property(e => e.TrackingNumber)
                .HasMaxLength(50)
                .IsUnicode(false)
                .HasColumnName("tracking_number");
        });

        modelBuilder.Entity<User>(entity =>
        {
            entity.HasKey(e => e.UserId).HasName("PK__Users__B9BE370F52734709");

            entity.HasIndex(e => e.Email, "UQ__Users__AB6E6164A83EBC91").IsUnique();

            entity.Property(e => e.UserId).HasColumnName("user_id");
            entity.Property(e => e.Email)
                .HasMaxLength(150)
                .IsUnicode(false)
                .HasColumnName("email");
            entity.Property(e => e.Name)
                .HasMaxLength(100)
                .IsUnicode(false)
                .HasColumnName("name");
            entity.Property(e => e.RoleId).HasColumnName("role_id");

            entity.HasOne(d => d.Role).WithMany(p => p.Users)
                .HasForeignKey(d => d.RoleId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Users_Roles");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
