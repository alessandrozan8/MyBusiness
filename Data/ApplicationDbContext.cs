using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using MySql.EntityFrameworkCore;
using Business.Models;
using Microsoft.Extensions.Configuration; // Aggiungi questo namespace

namespace Business.Data;

public class ApplicationDbContext : IdentityDbContext
{
    private readonly IConfiguration _configuration; // Aggiungi questo campo

    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options, IConfiguration configuration) : base(options)
    {
        _configuration = configuration; // Inizializza il campo
    }

    public DbSet<Client> Clients { get; set; }
    public DbSet<Trip> Trips { get; set; }
    public DbSet<DailyExpense> DailyExpenses { get; set; }


    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        builder.Entity<Trip>()
            .HasOne(t => t.Client)
            .WithMany(c => c.Trips)
            .HasForeignKey(t => t.ClienteId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Entity<Trip>()
            .HasOne(t => t.RequestingUser)
            .WithMany()
            .HasForeignKey(t => t.RequestingUserId);

        // builder.Entity<DailyExpense>()
        //     .HasOne(de => de.Trip)
        //     .WithMany(t => t.DailyExpenses)
        //     .HasForeignKey(de => de.TripId);

        // builder.Entity<DailyExpense>()
        //     .HasOne(de => de.User)
        //     .WithMany() // Assuming AspNetUsers doesn't have a direct collection of expenses
        //     .HasForeignKey(de => de.UserId);
    }
}