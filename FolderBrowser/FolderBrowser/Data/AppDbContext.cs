using FolderBrowser.Models;
using Microsoft.EntityFrameworkCore;

namespace FolderBrowser.Data
{
    /// <summary>
    /// DbContext for Entity Framework
    /// </summary>
    public class AppDbContext : DbContext
    {
        public DbSet<Folder> Folders { get; set; } = null!;

        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {
            Database.EnsureCreated();
        }
    }
}
