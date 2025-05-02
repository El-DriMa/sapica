using Microsoft.EntityFrameworkCore;
using sapica_backend.Data.Models;
using sapica_backend.Data.Models.Auth;

namespace sapica_backend.Data
{
    public class ApplicationDbContext(
        DbContextOptions options) : DbContext(options)
    {
        public DbSet<City> City { get; set; }
        public DbSet<Country> Country { get; set; }
        public DbSet<Admin> Admin {  get; set; }
        public DbSet<AdoptionPost> AdoptionPost { get; set; }
        public DbSet<AdoptionRequest> AdoptionRequest { get; set; }
        public DbSet<Animal> Animal { get; set; }
        public DbSet<AnimalImage> AnimalImage { get; set; }
        public DbSet<Donation> Donation { get; set; }
        public DbSet<Favourite> Favourite { get; set; }
        public DbSet<Notification> Notification { get; set; }
        public DbSet<Shelter> Shelter { get; set; }
        public DbSet<User> User { get; set; }
        public DbSet<UserAccount> UserAccount { get; set; }
        public DbSet<RefreshToken> RefreshToken {  get; set; }
        public DbSet<Review> Review { get; set; }
        public DbSet<Questions> Questions { get; set; }
        public DbSet<Event> Event { get; set; }

        public DbSet<Sponsor> Sponsor { get; set; }
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            modelBuilder.Entity<UserAccount>().UseTpcMappingStrategy();


            foreach (var relationship in modelBuilder.Model.GetEntityTypes().SelectMany(e => e.GetForeignKeys()))
            {
                relationship.DeleteBehavior = DeleteBehavior.NoAction;
            }

            // opcija kod nasljeđivanja
            // modelBuilder.Entity<NekaBaznaKlasa>().UseTpcMappingStrategy();

            // Cascade delete
            modelBuilder.Entity<AdoptionPost>()
                .HasOne(ap => ap.Animal)
                .WithOne()
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Animal>()
                       .HasMany(a => a.Images)
                       .WithOne()  
                       .HasForeignKey(ai => ai.AnimalId)
                       .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<UserAccount>().UseTpcMappingStrategy();

        }
    }
}
