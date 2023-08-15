using ArtWork.Database.Tables;

using Microsoft.EntityFrameworkCore;

namespace ArtWork.Database;

public class ArtWorkDbContext : DbContext
{
    public ArtWorkDbContext()
    {
        Database.EnsureCreated();
    }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        var DirPath = @$"{Settings.ArtWorkDirectory}\DataBase-DO-NOT-REMOVE";
        if (!Directory.Exists(DirPath))
        {
            Directory.CreateDirectory(DirPath);
        }
        var dbFile = @$"{DirPath}\ArtWork.db";
        optionsBuilder.UseSqlite($"Data Source={dbFile}");
    }

    public DbSet<Art> Arts { get; set; }
    public DbSet<Nude> Nudes { get; set; }
}
