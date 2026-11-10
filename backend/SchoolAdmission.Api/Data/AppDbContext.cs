using Microsoft.EntityFrameworkCore;
using SchoolAdmission.Api.Domain;

namespace SchoolAdmission.Api.Data;

public class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
{
    public DbSet<SchoolClass> Classes => Set<SchoolClass>();
    public DbSet<Hold> Holds => Set<Hold>();

    protected override void OnModelCreating(ModelBuilder b)
    {
        b.Entity<SchoolClass>()
            .ToTable("Classes", t => t.HasCheckConstraint("ck_classes_seats", "\"TotalSeats\" > 0"));

        b.Entity<Hold>(e =>
        {
            e.HasOne(h => h.Class).WithMany().HasForeignKey(h => h.ClassId);
            e.HasIndex(h => new { h.ClassId, h.Status });

            // One active application per student per class
            e.HasIndex(h => new { h.ClassId, h.StudentId })
             .HasFilter("\"Status\" = 'HELD'")
             .IsUnique();
        });
    }
}