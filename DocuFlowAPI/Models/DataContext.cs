using Microsoft.EntityFrameworkCore;

namespace DocuFlowAPI.Models
{
    public class DataContext : DbContext
    {
        public DataContext(DbContextOptions<DataContext> options) : base(options) { }

        public DbSet<User> Users { get; set; } = null!;

        public DbSet<Document> Documents => Set<Document>();
    }

}
