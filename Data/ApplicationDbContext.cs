using AcxiomCRM.Models;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace AcxiomCRM.Data
{
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public DbSet<Customer> Customers { get; set; } = null!;
        public DbSet<Lead> Leads { get; set; } = null!;
        public DbSet<Opportunity> Opportunities { get; set; } = null!;
        public DbSet<FollowUp> FollowUps { get; set; } = null!;
        public DbSet<Activity> Activities { get; set; } = null!;
        public DbSet<AuditLog> AuditLogs { get; set; } = null!;

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            // Indexes for Email & Phone Uniqueness
            builder.Entity<Customer>()
                .HasIndex(c => c.Email)
                .IsUnique();

            builder.Entity<Customer>()
                .HasIndex(c => c.Phone)
                .IsUnique();

            builder.Entity<Lead>()
                .HasIndex(l => l.Email);

            // Configure Decimal precision
            builder.Entity<Lead>()
                .Property(l => l.ExpectedValue)
                .HasColumnType("decimal(18,2)");

            builder.Entity<Opportunity>()
                .Property(o => o.Amount)
                .HasColumnType("decimal(18,2)");

            // Relationships & Delete Behavior (Restrict / SetNull to preserve business audit history)
            builder.Entity<Customer>()
                .HasOne(c => c.AssignedTo)
                .WithMany(u => u.AssignedCustomers)
                .HasForeignKey(c => c.AssignedToUserId)
                .OnDelete(DeleteBehavior.SetNull);

            builder.Entity<Lead>()
                .HasOne(l => l.AssignedTo)
                .WithMany(u => u.AssignedLeads)
                .HasForeignKey(l => l.AssignedToUserId)
                .OnDelete(DeleteBehavior.SetNull);

            builder.Entity<Opportunity>()
                .HasOne(o => o.Customer)
                .WithMany(c => c.Opportunities)
                .HasForeignKey(o => o.CustomerId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<Opportunity>()
                .HasOne(o => o.Lead)
                .WithMany(l => l.Opportunities)
                .HasForeignKey(o => o.LeadId)
                .OnDelete(DeleteBehavior.SetNull);

            builder.Entity<Opportunity>()
                .HasOne(o => o.AssignedTo)
                .WithMany(u => u.AssignedOpportunities)
                .HasForeignKey(o => o.AssignedToUserId)
                .OnDelete(DeleteBehavior.SetNull);

            builder.Entity<FollowUp>()
                .HasOne(f => f.Customer)
                .WithMany(c => c.FollowUps)
                .HasForeignKey(f => f.CustomerId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<FollowUp>()
                .HasOne(f => f.Lead)
                .WithMany(l => l.FollowUps)
                .HasForeignKey(f => f.LeadId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<FollowUp>()
                .HasOne(f => f.AssignedTo)
                .WithMany(u => u.AssignedFollowUps)
                .HasForeignKey(f => f.AssignedToUserId)
                .OnDelete(DeleteBehavior.SetNull);

            builder.Entity<Activity>()
                .HasOne(a => a.Customer)
                .WithMany(c => c.Activities)
                .HasForeignKey(a => a.CustomerId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<Activity>()
                .HasOne(a => a.Lead)
                .WithMany(l => l.Activities)
                .HasForeignKey(a => a.LeadId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<Activity>()
                .HasOne(a => a.AssignedTo)
                .WithMany(u => u.AssignedActivities)
                .HasForeignKey(a => a.AssignedToUserId)
                .OnDelete(DeleteBehavior.SetNull);
        }
    }
}
