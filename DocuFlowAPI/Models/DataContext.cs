using Microsoft.EntityFrameworkCore;

namespace DocuFlowAPI.Models
{
    //kontekst baze podataka (EF Core)
    public class DataContext : DbContext
    {
        public DataContext(DbContextOptions<DataContext> options) : base(options) { }      //DbContextOptions - sadrzi db konfiguraciju


        //// DbSet predstavlja tabelu u bazi podataka za entitet User
        public DbSet<User> Users { get; set; } = null!;
        public DbSet<Document> Documents => Set<Document>();

        public DbSet<Comment> Comments => Set<Comment>();
        //public DbSet<Comment> Comments { get; set; } = null!;


        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Enum kao string (umesto int)
            modelBuilder.Entity<User>()
                .Property(u => u.Role)
                .HasConversion<string>();
        }
    }
}
