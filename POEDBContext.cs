using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using ST10092132.Models;

namespace ST10092132.Data;

public partial class POEDBContext : DbContext
{
    public POEDBContext()
    {
    }

    public POEDBContext(DbContextOptions<POEDBContext> options)
        : base(options)
    {
    }

    public virtual DbSet<Booking> Bookings { get; set; }

    public virtual DbSet<Event> Events { get; set; }

    public virtual DbSet<Venue> Venues { get; set; }

    // Add DbSet for EventType
    public virtual DbSet<EventType> EventTypes { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Booking>(entity =>
        {
            entity.HasKey(e => e.BookingId).HasName("PK__Booking__73951AEDA8D51832");

            entity.ToTable("Booking");

            entity.Property(e => e.BookingDate).HasColumnType("datetime");

            // Add unique constraint to prevent double bookings
            entity.HasIndex(e => new { e.VenueId, e.BookingDate })
                .IsUnique()
                .HasDatabaseName("IX_Unique_Venue_BookingDate");

            entity.HasOne(d => d.Event).WithMany(p => p.Bookings)
                .HasForeignKey(d => d.EventId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Booking__EventId__45F365D3");

            entity.HasOne(d => d.Venue).WithMany(p => p.Bookings)
                .HasForeignKey(d => d.VenueId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Booking__VenueId__46E78A0C");
        });

        modelBuilder.Entity<Event>(entity =>
        {
            entity.HasKey(e => e.EventId).HasName("PK__Event__7944C8102567E777");

            entity.ToTable("Event");

            entity.Property(e => e.EventDate).HasColumnType("datetime");
            entity.Property(e => e.EventName).HasMaxLength(100);

            // Add relationship to EventType
            entity.HasOne(e => e.EventType)
                .WithMany(et => et.Events)
                .HasForeignKey(e => e.EventTypeId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Event_EventType");
        });

        modelBuilder.Entity<Venue>(entity =>
        {
            entity.HasKey(e => e.VenueId).HasName("PK__Venue__3C57E5F27B9CFE3B");

            entity.ToTable("Venue");

            entity.Property(e => e.ImageUrl).HasMaxLength(255);
            entity.Property(e => e.Location).HasMaxLength(255);
            entity.Property(e => e.VenueName).HasMaxLength(100);

            
        });

        // Configure EventType entity
        modelBuilder.Entity<EventType>(entity =>
        {
            entity.HasKey(e => e.EventTypeId).HasName("PK_EventType");

            entity.ToTable("EventType");

            entity.Property(e => e.EventTypeName)
                .IsRequired()
                .HasMaxLength(50);
        });

        // Seed initial event types
        modelBuilder.Entity<EventType>().HasData(
            new EventType { EventTypeId = 1, EventTypeName = "Conference" },
            new EventType { EventTypeId = 2, EventTypeName = "Wedding" },
            new EventType { EventTypeId = 3, EventTypeName = "Concert" },
            new EventType { EventTypeId = 4, EventTypeName = "Seminar" },
            new EventType { EventTypeId = 5, EventTypeName = "Party" }
        );

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}