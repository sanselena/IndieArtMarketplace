﻿using Microsoft.EntityFrameworkCore;
using IndieArtMarketplace.Models;
using Microsoft.AspNetCore.DataProtection.EntityFrameworkCore;

namespace IndieArtMarketplace.DAL
{
    public class AppDbContext : DbContext, IDataProtectionKeyContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }
        
        public DbSet<User> Users { get; set; }
        public DbSet<Artwork> Artworks { get; set; }
        public DbSet<MusicTrack> MusicTracks { get; set; }
        public DbSet<Transaction> Transactions { get; set; }
        public DbSet<ClickLog> ClickLogs { get; set; }
        public DbSet<UploadLog> UploadLogs { get; set; }
        public DbSet<DataProtectionKey> DataProtectionKeys { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<User>().ToTable("users", schema: "public");
            modelBuilder.Entity<Artwork>().ToTable("artworks", schema: "public");
            modelBuilder.Entity<MusicTrack>().ToTable("musictracks", schema: "public");
            modelBuilder.Entity<Transaction>().ToTable("transactions", schema: "public");
            modelBuilder.Entity<ClickLog>().ToTable("click_logs", schema: "public");
            modelBuilder.Entity<UploadLog>().ToTable("upload_logs", schema: "public");

            modelBuilder.Entity<User>(entity =>
            {
                entity.HasKey(e => e.UserID);
                entity.Property(e => e.UserID).HasColumnName("userid");
                entity.Property(e => e.Username).HasColumnType("text").IsRequired().HasColumnName("username");
                entity.Property(e => e.Email).HasColumnType("text").IsRequired().HasColumnName("email");
                entity.Property(e => e.Password).HasColumnType("text").IsRequired().HasColumnName("password");
                entity.Property(e => e.Role).HasColumnType("text").IsRequired().HasColumnName("role");
                entity.Property(e => e.RegistrationDate).HasColumnType("timestamp with time zone").IsRequired().HasColumnName("registrationdate");
            });

            modelBuilder.Entity<Artwork>(entity =>
            {
                entity.HasKey(e => e.ArtworkID);
                entity.Property(e => e.ArtworkID).HasColumnName("artworkid");
                entity.Property(e => e.Title).HasColumnType("text").IsRequired().HasColumnName("title");
                entity.Property(e => e.Description).HasColumnType("text").HasColumnName("description");
                entity.Property(e => e.Price).HasColumnType("decimal(18,2)").IsRequired().HasColumnName("price");
                entity.Property(e => e.FileURL).HasColumnType("text").IsRequired().HasColumnName("fileurl");
                entity.Property(e => e.UploadDate).HasColumnType("timestamp with time zone").IsRequired().HasColumnName("uploaddate");
                entity.Property(e => e.ArtistID).IsRequired().HasColumnName("artistid");
                entity.Property(e => e.License).HasColumnName("license");
                entity.HasOne<User>()
                    .WithMany()
                    .HasForeignKey(e => e.ArtistID)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            modelBuilder.Entity<MusicTrack>(entity =>
            {
                entity.HasKey(e => e.TrackID);
                entity.Property(e => e.TrackID).HasColumnName("trackid");
                entity.Property(e => e.Title).HasColumnType("text").IsRequired().HasColumnName("title");
                entity.Property(e => e.Description).HasColumnType("text").HasColumnName("description");
                entity.Property(e => e.Price).HasColumnType("decimal(18,2)").IsRequired().HasColumnName("price");
                entity.Property(e => e.FileURL).HasColumnType("text").IsRequired().HasColumnName("fileurl");
                entity.Property(e => e.UploadDate).HasColumnType("timestamp with time zone").IsRequired().HasColumnName("uploaddate");
                entity.Property(e => e.ArtistID).IsRequired().HasColumnName("artistid");
                entity.Property(e => e.License).HasColumnName("license");
                entity.HasOne<User>()
                    .WithMany()
                    .HasForeignKey(e => e.ArtistID)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            modelBuilder.Entity<Transaction>(entity =>
            {
                entity.HasKey(e => e.TransactionID);
                entity.Property(e => e.TransactionID).HasColumnName("transactionid");
                entity.Property(e => e.BuyerID).IsRequired().HasColumnName("buyerid");
                entity.Property(e => e.ArtworkID).HasColumnName("artworkid");
                entity.Property(e => e.TrackID).HasColumnName("trackid");
                entity.Property(e => e.PurchaseDate).HasColumnType("timestamp with time zone").IsRequired().HasColumnName("purchasedate");
                entity.Property(e => e.Amount).HasColumnType("decimal(18,2)").IsRequired().HasColumnName("amount");
                entity.Property(e => e.Status).HasColumnType("text").IsRequired().HasColumnName("status");

                entity.HasOne<User>()
                    .WithMany()
                    .HasForeignKey(e => e.BuyerID)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne<Artwork>()
                    .WithMany()
                    .HasForeignKey(e => e.ArtworkID);

                entity.HasOne<MusicTrack>()
                    .WithMany()
                    .HasForeignKey(e => e.TrackID);

                entity.HasIndex(e => new { e.ArtworkID, e.TrackID }).IsUnique();
            });

            modelBuilder.Entity<ClickLog>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Id).HasColumnName("id");
                entity.Property(e => e.ButtonType).HasColumnName("button_type");
                entity.Property(e => e.ClickedAt).HasColumnName("clicked_at");
                entity.Property(e => e.UserId).HasColumnName("user_id");
                entity.Property(e => e.SessionId).HasColumnName("session_id");
            });

            modelBuilder.Entity<UploadLog>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Id).HasColumnName("id");
                entity.Property(e => e.UserId).HasColumnName("user_id");
                entity.Property(e => e.ContentType).HasColumnName("content_type");
                entity.Property(e => e.UploadedAt).HasColumnName("uploaded_at");
                entity.Property(e => e.Title).HasColumnName("title");
                entity.Property(e => e.Price).HasColumnType("numeric(10,2)").HasColumnName("price");
            });
        }
    }
}