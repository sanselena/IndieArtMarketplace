using Microsoft.EntityFrameworkCore;
using IndieArtMarketplace.Models;

namespace IndieArtMarketplace.DAL
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }
        
        public DbSet<User> Users { get; set; }
        public DbSet<Artwork> Artworks { get; set; }
        public DbSet<MusicTrack> MusicTracks { get; set; }
        public DbSet<Transaction> Transactions { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Configure User
            modelBuilder.Entity<User>(entity =>
            {
                entity.HasKey(e => e.UserID);
                entity.Property(e => e.Username).IsRequired();
                entity.Property(e => e.Email).IsRequired();
                entity.Property(e => e.Password).IsRequired();
                entity.Property(e => e.Role).IsRequired();
            });

            // Configure Artwork
            modelBuilder.Entity<Artwork>(entity =>
            {
                entity.HasKey(e => e.ArtworkID);
                entity.Property(e => e.Title).IsRequired();
                entity.Property(e => e.Description).IsRequired();
                entity.Property(e => e.Price).HasColumnType("decimal(18,2)");
                entity.HasOne<User>()
                    .WithMany()
                    .HasForeignKey(e => e.ArtistID);
            });

            // Configure MusicTrack
            modelBuilder.Entity<MusicTrack>(entity =>
            {
                entity.HasKey(e => e.TrackID);
                entity.Property(e => e.Title).IsRequired();
                entity.Property(e => e.Description).IsRequired();
                entity.Property(e => e.Price).HasColumnType("decimal(18,2)");
                entity.HasOne<User>()
                    .WithMany()
                    .HasForeignKey(e => e.ArtistID);
            });

            // Configure Transaction
            modelBuilder.Entity<Transaction>(entity =>
            {
                entity.HasKey(e => e.TransactionID);
                entity.Property(e => e.Amount).HasColumnType("decimal(18,2)");
                entity.HasOne<User>()
                    .WithMany()
                    .HasForeignKey(e => e.BuyerID);
            });
        }
    }
}