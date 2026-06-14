using Microsoft.EntityFrameworkCore;

namespace MedicalAppAPI;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<Patient> Patients { get; set; }
    public DbSet<User> Users { get; set; } 
}