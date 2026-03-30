using Microsoft.EntityFrameworkCore;
using PulseDesk.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PulseDesk.Infrastructure.Persistence
{
    public  class PulseDeskDbContext: DbContext
    {
        public PulseDeskDbContext(DbContextOptions<PulseDeskDbContext> options) : base(options) { }

        public DbSet<Role> Roles => Set<Role>();
        public DbSet<User> Users => Set<User>();
        public DbSet<Policy> Policies => Set<Policy>();
        public DbSet<RolePolicy> RolePolicies => Set<RolePolicy>();
        public DbSet<RefreshToken> RefreshTokens => Set<RefreshToken>();
        public DbSet<AuditLog> AuditLogs => Set<AuditLog>();
        public DbSet<Notification> Notifications => Set<Notification>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // All tables live in the PulseDesk schema
            modelBuilder.HasDefaultSchema("PulseDesk");

            // ── User ──────────────────────────────────────────────
            modelBuilder.Entity<User>(entity =>
            {
                entity.ToTable("Users");
                entity.HasIndex(u => u.Email).IsUnique();

                // FullName is a computed C# property — not a DB column
                entity.Ignore(u => u.FullName);

                entity.HasOne(u => u.Role)
                      .WithMany(r => r.Users)
                      .HasForeignKey(u => u.RoleId)
                      .OnDelete(DeleteBehavior.Restrict);
            });

            modelBuilder.Entity<Role>(entity =>
            {
                entity.ToTable("Roles");
                entity.HasIndex(r => r.Name).IsUnique();
            });
            modelBuilder.Entity<Policy>(e =>
            {
                e.ToTable("Policies");
                e.HasIndex(r => r.Name).IsUnique();
            });

            // ── RolePolicy (composite key) ────────────────────────
            modelBuilder.Entity<RolePolicy>(entity =>
            {
                entity.ToTable("RolePolicies");
                entity.HasKey(rp => new { rp.RoleId, rp.PolicyId });

                entity.HasOne(rp => rp.Role)
                      .WithMany(r => r.RolePolicies)
                      .HasForeignKey(rp => rp.RoleId)
                      .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(rp => rp.Policy)
                      .WithMany(p => p.RolePolicies)
                      .HasForeignKey(rp => rp.PolicyId)
                      .OnDelete(DeleteBehavior.Cascade);
            });

            // ── RefreshToken ──────────────────────────────────────
            modelBuilder.Entity<RefreshToken>(entity =>
            {
                entity.ToTable("RefreshTokens");
                entity.HasIndex(rt => rt.Token).IsUnique();

                // Domain properties — not DB columns
                entity.Ignore(rt => rt.IsExpired);
                entity.Ignore(rt => rt.IsActive);

                entity.HasOne(rt => rt.User)
                      .WithMany(u => u.RefreshTokens)
                      .HasForeignKey(rt => rt.UserId)
                      .OnDelete(DeleteBehavior.Cascade);
            });

            // ── AuditLog ──────────────────────────────────────────
            modelBuilder.Entity<AuditLog>(entity =>
            {
                entity.ToTable("AuditLogs");

                entity.HasOne(al => al.User)
                      .WithMany(u => u.AuditLogs)
                      .HasForeignKey(al => al.UserId)
                      .OnDelete(DeleteBehavior.SetNull);
            });

            // ── Notification ──────────────────────────────────────
            modelBuilder.Entity<Notification>(entity =>
            {
                entity.ToTable("Notifications");

                entity.HasOne(n => n.User)
                      .WithMany(u => u.Notifications)
                      .HasForeignKey(n => n.UserId)
                      .OnDelete(DeleteBehavior.Cascade);
            });
        }
    }
}
