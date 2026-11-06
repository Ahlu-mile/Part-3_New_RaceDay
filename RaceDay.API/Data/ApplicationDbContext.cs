using Microsoft.EntityFrameworkCore;
using RaceDay.API.Models;

namespace RaceDay.API.Data
{
    /// <summary>
    /// Code-First EF Core context. The schema configured here matches the
    /// Part 1 ERD and RaceDay_Database.sql exactly: six entities, the same
    /// primary/foreign keys, and the same constraints (CHECK, UNIQUE, DEFAULT).
    /// </summary>
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options) { }

        public DbSet<User> Users => Set<User>();
        public DbSet<Event> Events => Set<Event>();
        public DbSet<Category> Categories => Set<Category>();
        public DbSet<Enrolment> Enrolments => Set<Enrolment>();
        public DbSet<Result> Results => Set<Result>();
        public DbSet<RouteInfo> RouteInfos => Set<RouteInfo>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // ---------- Users ----------
            modelBuilder.Entity<User>(entity =>
            {
                entity.HasKey(u => u.UserId);
                entity.HasIndex(u => u.Email).IsUnique();
                entity.Property(u => u.Role).HasMaxLength(20);
                entity.ToTable(t => t.HasCheckConstraint(
                    "CK_Users_Role", "[Role] IN ('Organiser','Participant')"));
            });

            // ---------- Events ----------
            modelBuilder.Entity<Event>(entity =>
            {
                entity.HasKey(e => e.EventId);
                entity.HasOne(e => e.Organiser)
                      .WithMany(u => u.OrganisedEvents)
                      .HasForeignKey(e => e.OrganiserId)
                      .OnDelete(DeleteBehavior.Restrict);
                entity.ToTable(t => t.HasCheckConstraint(
                    "CK_Events_Type", "[EventType] IN ('Run','Walk','Cycle')"));
            });

            // ---------- Categories ----------
            modelBuilder.Entity<Category>(entity =>
            {
                entity.HasKey(c => c.CategoryId);
                entity.HasOne(c => c.Event)
                      .WithMany(e => e.Categories)
                      .HasForeignKey(c => c.EventId)
                      .OnDelete(DeleteBehavior.Cascade);
            });

            // ---------- Enrolments ----------
            modelBuilder.Entity<Enrolment>(entity =>
            {
                entity.HasKey(en => en.EnrolmentId);
                entity.HasOne(en => en.Participant)
                      .WithMany(u => u.Enrolments)
                      .HasForeignKey(en => en.ParticipantId)
                      .OnDelete(DeleteBehavior.Restrict);
                entity.HasOne(en => en.Category)
                      .WithMany(c => c.Enrolments)
                      .HasForeignKey(en => en.CategoryId)
                      .OnDelete(DeleteBehavior.Restrict);
                entity.HasIndex(en => new { en.ParticipantId, en.CategoryId }).IsUnique();
                entity.ToTable(t => t.HasCheckConstraint(
                    "CK_Enrolments_Status", "[Status] IN ('Pending','Confirmed','Cancelled')"));
            });

            // ---------- Results ----------
            modelBuilder.Entity<Result>(entity =>
            {
                entity.HasKey(r => r.ResultId);
                entity.HasOne(r => r.Enrolment)
                      .WithOne(en => en.Result)
                      .HasForeignKey<Result>(r => r.EnrolmentId)
                      .OnDelete(DeleteBehavior.Restrict);
                entity.HasIndex(r => r.EnrolmentId).IsUnique();
                entity.HasOne(r => r.CapturedByUser)
                      .WithMany(u => u.CapturedResults)
                      .HasForeignKey(r => r.CapturedByUserId)
                      .OnDelete(DeleteBehavior.Restrict);
            });

            // ---------- RouteInfo ----------
            modelBuilder.Entity<RouteInfo>(entity =>
            {
                entity.HasKey(ri => ri.RouteId);
                entity.HasOne(ri => ri.Event)
                      .WithMany(e => e.Routes)
                      .HasForeignKey(ri => ri.EventId)
                      .OnDelete(DeleteBehavior.Cascade);
            });
        }
    }
}
