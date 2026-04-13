using Microsoft.EntityFrameworkCore;
using PressSubscription.Models;
using System.IO;

namespace PressSubscription.Data;

public class AppDbContext : DbContext
{
    public DbSet<Subscriber> Subscribers => Set<Subscriber>();
    public DbSet<Publication> Publications => Set<Publication>();
    public DbSet<Subscription> Subscriptions => Set<Subscription>();
    public DbSet<User> Users => Set<User>();

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        var folder = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "data");
        Directory.CreateDirectory(folder);

        var dbPath = Path.Combine(folder, "app.db");

        optionsBuilder.UseSqlite($"Data Source={dbPath}");
    }
}