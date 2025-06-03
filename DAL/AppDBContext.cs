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

            // Tablo isimlerini PostgreSQL'deki mevcut tablo isimleriyle eşleştir
            modelBuilder.Entity<User>().ToTable("users", schema: "public");
            modelBuilder.Entity<Artwork>().ToTable("artworks", schema: "public");
            modelBuilder.Entity<MusicTrack>().ToTable("musictracks", schema: "public");
            modelBuilder.Entity<Transaction>().ToTable("transactions", schema: "public");

            // Configure User
            modelBuilder.Entity<User>(entity =>
            {
                entity.HasKey(e => e.UserID);
                entity.Property(e => e.UserID).HasColumnName("userid");
                entity.Property(e => e.Username).IsRequired().HasColumnName("username");
                entity.Property(e => e.Email).IsRequired().HasColumnName("email");
                entity.Property(e => e.Password).IsRequired().HasColumnName("password");
                entity.Property(e => e.Role).IsRequired().HasColumnName("role");
                entity.Property(e => e.RegistrationDate).HasColumnName("registrationdate");
            });

            // Configure Artwork
            modelBuilder.Entity<Artwork>(entity =>
            {
                entity.HasKey(e => e.ArtworkID);
                entity.Property(e => e.ArtworkID).HasColumnName("artworkid");
                entity.Property(e => e.Title).IsRequired().HasColumnName("title");
                entity.Property(e => e.Description).HasColumnName("description");
                entity.Property(e => e.Price).HasColumnType("decimal(10,2)").IsRequired().HasColumnName("price");
                entity.Property(e => e.FileURL).IsRequired().HasColumnName("fileurl");
                entity.Property(e => e.UploadDate).HasColumnName("uploaddate");
                entity.Property(e => e.ArtistID).IsRequired().HasColumnName("artistid");
                entity.Property(e => e.License).HasColumnName("license");
                entity.HasOne<User>()
                    .WithMany()
                    .HasForeignKey(e => e.ArtistID);
            });

            // Configure MusicTrack
            modelBuilder.Entity<MusicTrack>(entity =>
            {
                entity.HasKey(e => e.TrackID);
                entity.Property(e => e.TrackID).HasColumnName("trackid");
                entity.Property(e => e.Title).IsRequired().HasColumnName("title");
                entity.Property(e => e.Description).HasColumnName("description");
                entity.Property(e => e.Price).HasColumnType("decimal(10,2)").IsRequired().HasColumnName("price");
                entity.Property(e => e.FileURL).IsRequired().HasColumnName("fileurl");
                entity.Property(e => e.UploadDate).HasColumnName("uploaddate");
                entity.Property(e => e.ArtistID).IsRequired().HasColumnName("artistid");
                entity.Property(e => e.License).HasColumnName("license");
                entity.HasOne<User>()
                    .WithMany()
                    .HasForeignKey(e => e.ArtistID);
            });

            // Configure Transaction
            modelBuilder.Entity<Transaction>(entity =>
            {
                entity.HasKey(e => e.TransactionID);
                entity.Property(e => e.TransactionID).HasColumnName("transactionid");
                entity.Property(e => e.BuyerID).IsRequired().HasColumnName("buyerid");
                entity.Property(e => e.ArtworkID).HasColumnName("artworkid");
                entity.Property(e => e.TrackID).HasColumnName("trackid");
                entity.Property(e => e.PurchaseDate).HasColumnName("purchasedate");
                entity.Property(e => e.Amount).HasColumnType("decimal(10,2)").IsRequired().HasColumnName("amount");

                entity.HasOne<User>()
                    .WithMany()
                    .HasForeignKey(e => e.BuyerID);

                entity.HasOne<Artwork>()
                    .WithMany()
                    .HasForeignKey(e => e.ArtworkID);

                entity.HasOne<MusicTrack>()
                    .WithMany()
                    .HasForeignKey(e => e.TrackID);

                entity.HasIndex(e => new { e.ArtworkID, e.TrackID }).IsUnique();
            });
        }
    }
}