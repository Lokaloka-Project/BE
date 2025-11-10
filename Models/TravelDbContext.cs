using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;

namespace TravelAgencyAPI.Models;

public partial class TravelDbContext : DbContext
{
    public TravelDbContext()
    {
    }

    public TravelDbContext(DbContextOptions<TravelDbContext> options)
        : base(options)
    {
    }

    public virtual DbSet<Image> Images { get; set; }

    public virtual DbSet<Location> Locations { get; set; }
    public DbSet<LocationTranslation> LocationTranslations { get; set; }

    public virtual DbSet<LocationDetail> LocationDetails { get; set; }

    public virtual DbSet<Rating> Ratings { get; set; }

    public virtual DbSet<Tour> Tours { get; set; }

    public virtual DbSet<TourLocation> TourLocations { get; set; }

    public virtual DbSet<User> Users { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
#warning To protect potentially sensitive information in your connection string, you should move it out of source code. You can avoid scaffolding the connection string by using the Name= syntax to read it from configuration - see https://go.microsoft.com/fwlink/?linkid=2131148. For more guidance on storing connection strings, see https://go.microsoft.com/fwlink/?LinkId=723263.
        => optionsBuilder.UseSqlServer("Server=PHATDEPTRAI2004\\PHATDEPTRAIVAI2;Database=TravelDB;Trusted_Connection=True;TrustServerCertificate=True;");

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Image>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Images__3214EC07AD195F3E");

            entity.Property(e => e.Image1)
                .HasMaxLength(200)
                .HasColumnName("Image");

            entity.HasOne(d => d.Rating).WithMany(p => p.Images)
                .HasForeignKey(d => d.RatingId)
                .HasConstraintName("FK__Images__RatingId__3D5E1FD2");

            modelBuilder.Entity<Location>()
           .HasMany(l => l.Translations)
           .WithOne(t => t.Location)
           .HasForeignKey(t => t.LocationId)
           .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<LocationTranslation>()
                .HasIndex(t => new { t.LocationId, t.Language })
                .IsUnique();
        });

        modelBuilder.Entity<Location>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Location__3214EC0701F88F5D");

            entity.Property(e => e.City).HasMaxLength(100);
            entity.Property(e => e.CoordinateX).HasColumnType("decimal(9, 6)");
            entity.Property(e => e.CoordinateY).HasColumnType("decimal(9, 6)");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.Name).HasMaxLength(150);
            entity.Property(e => e.Type).HasMaxLength(100);
        });

        modelBuilder.Entity<LocationDetail>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Location__3214EC07D4417382");

            entity.Property(e => e.Image).HasMaxLength(200);
            entity.Property(e => e.ServiceAround).HasMaxLength(255);
            entity.Property(e => e.Ticket).HasColumnType("decimal(10, 2)");
            entity.Property(e => e.UpdatedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");

            entity.HasOne(d => d.Location).WithMany(p => p.LocationDetails)
                .HasForeignKey(d => d.LocationId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__LocationD__Locat__2D27B809");
        });

        modelBuilder.Entity<Rating>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Ratings__3214EC070A4C58A9");

            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.RatingValue).HasColumnType("decimal(2, 1)");

            entity.HasOne(d => d.Location).WithMany(p => p.Ratings)
                .HasForeignKey(d => d.LocationId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Ratings__Locatio__3A81B327");

            entity.HasOne(d => d.User).WithMany(p => p.Ratings)
                .HasForeignKey(d => d.UserId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Ratings__UserId__398D8EEE");
            entity.HasOne(r => r.Location).WithMany(l => l.Ratings).HasForeignKey(r => r.LocationId);
        });

        modelBuilder.Entity<Tour>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Tours__3214EC07D5AFDDFC");

            entity.Property(e => e.Address).HasMaxLength(200);
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.Duration).HasMaxLength(50);
            entity.Property(e => e.Price).HasColumnType("decimal(12, 2)");
            entity.Property(e => e.Status).HasDefaultValue(true);
            entity.Property(e => e.Title).HasMaxLength(200);

            entity.HasOne(d => d.User).WithMany(p => p.Tours)
                .HasForeignKey(d => d.UserId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Tours__UserId__31EC6D26");
        });

        modelBuilder.Entity<TourLocation>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__TourLoca__3214EC07D1C11197");

            entity.Property(e => e.Note).HasMaxLength(255);

            entity.HasOne(d => d.Location).WithMany(p => p.TourLocations)
                .HasForeignKey(d => d.LocationId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__TourLocat__Locat__35BCFE0A");

            entity.HasOne(d => d.Tour).WithMany(p => p.TourLocations)
                .HasForeignKey(d => d.TourId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__TourLocat__TourI__34C8D9D1");
        });

        modelBuilder.Entity<User>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Users__3214EC071A214DBB");

            entity.HasIndex(e => e.Email, "UQ__Users__A9D105340472FEA7").IsUnique();

            entity.Property(e => e.Avatar).HasMaxLength(200);
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.Email).HasMaxLength(100);
            entity.Property(e => e.Interest).HasMaxLength(255);
            entity.Property(e => e.IsActive).HasDefaultValue(true);
            entity.Property(e => e.Name).HasMaxLength(100);
            entity.Property(e => e.Password).HasMaxLength(100);
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
