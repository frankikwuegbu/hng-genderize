using Application.Common.Interfaces;
using Domain.Entity;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure;

public class ApplicationDbContext : DbContext, IApplicationDbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
    }

    public DbSet<Profile> Profiles => Set<Profile>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<Profile>(builder =>
        {
            builder.HasIndex(profile => profile.Name).IsUnique();
            builder.Property(profile => profile.Name).HasMaxLength(200);
            builder.Property(profile => profile.Gender).HasMaxLength(50);
            builder.Property(profile => profile.AgeGroup).HasMaxLength(50);
            builder.Property(profile => profile.CountryId).HasMaxLength(10);
            builder.Property(profile => profile.CountryName).HasMaxLength(200);
        });
    }
}
