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
                entity.HasKey(e => e.UserID).HasName("userid");
                entity.Property(e => e.Username).IsRequired().HasColumnName("username");
                entity.Property(e => e.Email).IsRequired().HasColumnName("email");
                entity.Property(e => e.Password).IsRequired().HasColumnName("password");
                entity.Property(e => e.Role).IsRequired().HasColumnName("role");
                entity.Property(e => e.RegistrationDate).HasColumnName("registrationdate");
            });

            // Configure Artwork
            modelBuilder.Entity<Artwork>(entity =>
            {
                entity.HasKey(e => e.ArtworkID).HasName("artworkid");
                entity.Property(e => e.Title).IsRequired().HasColumnName("title");
                entity.Property(e => e.Description).IsRequired().HasColumnName("description");
                entity.Property(e => e.Price).HasColumnType("decimal(10,2)").IsRequired().HasColumnName("price");
                entity.Property(e => e.FileURL).IsRequired().HasColumnName("fileurl");
                entity.Property(e => e.UploadDate).HasColumnName("uploaddate");
                entity.Property(e => e.ArtistID).IsRequired().HasColumnName("artistid");
                entity.Property(e => e.License).HasColumnName("license");
                entity.HasOne<User>()
                    .WithMany()
                    .HasForeignKey(e => e.ArtistID)
                    .HasConstraintName("fk_artworks_users_artistid");
            });

            // Configure MusicTrack
            modelBuilder.Entity<MusicTrack>(entity =>
            {
                entity.HasKey(e => e.TrackID).HasName("trackid");
                entity.Property(e => e.Title).IsRequired().HasColumnName("title");
                entity.Property(e => e.Description).IsRequired().HasColumnName("description");
                entity.Property(e => e.Price).HasColumnType("decimal(10,2)").IsRequired().HasColumnName("price");
                entity.Property(e => e.FileURL).IsRequired().HasColumnName("fileurl");
                entity.Property(e => e.UploadDate).HasColumnName("uploaddate");
                entity.Property(e => e.ArtistID).IsRequired().HasColumnName("artistid");
                entity.Property(e => e.License).HasColumnName("license");
                entity.HasOne<User>()
                    .WithMany()
                    .HasForeignKey(e => e.ArtistID)
                    .HasConstraintName("fk_musictracks_users_artistid");
            });

            // Configure Transaction
            modelBuilder.Entity<Transaction>(entity =>
            {
                entity.HasKey(e => e.TransactionID).HasName("transactionid");
                entity.Property(e => e.BuyerID).IsRequired().HasColumnName("buyerid");
                entity.Property(e => e.ArtworkID).HasColumnName("artworkid");
                entity.Property(e => e.TrackID).HasColumnName("trackid");
                entity.Property(e => e.PurchaseDate).HasColumnName("purchasedate");
                entity.Property(e => e.Amount).HasColumnType("decimal(10,2)").IsRequired().HasColumnName("amount");

                entity.HasOne<User>()
                    .WithMany()
                    .HasForeignKey(e => e.BuyerID)
                    .HasConstraintName("fk_transactions_users_buyerid");

                entity.HasOne<Artwork>()
                    .WithMany()
                    .HasForeignKey(e => e.ArtworkID)
                    .HasConstraintName("fk_transactions_artworks_artworkid");

                entity.HasOne<MusicTrack>()
                    .WithMany()
                    .HasForeignKey(e => e.TrackID)
                    .HasConstraintName("fk_transactions_musictracks_trackid");
            });
        }
    }
}