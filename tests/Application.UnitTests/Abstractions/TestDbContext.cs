using Application.Abstractions.Data;
using Domain.Inquiries;
using Domain.Properties;
using Domain.Users;
using Microsoft.EntityFrameworkCore;

namespace Application.UnitTests.Abstractions;

/// <summary>
/// A lightweight in-memory <see cref="DbContext"/> that implements <see cref="IApplicationDbContext"/>
/// so Application handlers can be unit tested without referencing the Infrastructure layer.
/// </summary>
public sealed class TestDbContext(DbContextOptions<TestDbContext> options)
    : DbContext(options), IApplicationDbContext
{
    public DbSet<User> Users { get; set; }

    public DbSet<RefreshToken> RefreshTokens { get; set; }

    public DbSet<Property> Properties { get; set; }

    public DbSet<PropertyImage> PropertyImages { get; set; }

    public DbSet<Inquiry> Inquiries { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // Mirrors the owned-type mapping from Infrastructure's PropertyConfiguration.
        modelBuilder.Entity<Property>().OwnsOne(p => p.Address);
    }
}
