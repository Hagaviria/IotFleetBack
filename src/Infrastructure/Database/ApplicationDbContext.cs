using Microsoft.EntityFrameworkCore;
using Infrastructure.Database;
using Microsoft.Extensions.Configuration;
using System.IO;
using Domain.Models;
using Application.Abstractions.Data;

namespace Infrastructure.Database
{
    public class ApplicationDbContext : DbContext, IApplicationDbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
        { }
        public ApplicationDbContext() { }
        
        public DbSet<User> Users { get; set; }
        public DbSet<Vehicle> Vehicles { get; set; }
        public DbSet<SensorData> SensorData { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            
            ConfigureUser(modelBuilder);
            ConfigureVehicle(modelBuilder);
            ConfigureSensorData(modelBuilder);
        }
        
        
        private static void ConfigureUser(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<User>(entity =>
            {
                entity.HasKey(u => u.Id);
                
                entity.Property(u => u.Email)
                    .IsRequired()
                    .HasMaxLength(255);
                    
                entity.Property(u => u.PasswordHash)
                    .IsRequired();
                    
                entity.Property(u => u.FirstName)
                    .IsRequired()
                    .HasMaxLength(100);
                    
                entity.Property(u => u.LastName)
                    .IsRequired()
                    .HasMaxLength(100);
                
                entity.HasIndex(u => u.Email)
                    .IsUnique();
            });
        }
        
        private static void ConfigureVehicle(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Vehicle>(entity =>
            {
                entity.HasKey(v => v.Id);
                
                entity.Property(v => v.LicensePlate)
                    .IsRequired()
                    .HasMaxLength(20);
                    
                entity.Property(v => v.Model)
                    .IsRequired()
                    .HasMaxLength(100);
                    
                entity.Property(v => v.Brand)
                    .IsRequired()
                    .HasMaxLength(100);
                    
                entity.Property(v => v.FuelCapacity)
                    .HasPrecision(10, 2);
                    
                entity.Property(v => v.AverageConsumption)
                    .HasPrecision(10, 4);
                
                entity.HasIndex(v => v.LicensePlate)
                    .IsUnique();
                    
                entity.HasIndex(v => v.FleetId);
            });
        }
        
        private static void ConfigureSensorData(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<SensorData>(entity =>
            {
                entity.HasKey(s => s.Id);
                
                entity.Property(s => s.Latitude)
                    .HasPrecision(18, 15);
                    
                entity.Property(s => s.Longitude)
                    .HasPrecision(18, 15);
                    
                entity.Property(s => s.Altitude)
                    .HasPrecision(10, 2);
                    
                entity.Property(s => s.Speed)
                    .HasPrecision(8, 2);
                    
                entity.Property(s => s.FuelLevel)
                    .HasPrecision(5, 2);
                    
                entity.Property(s => s.FuelConsumption)
                    .HasPrecision(10, 4);
                    
                entity.Property(s => s.EngineTemperature)
                    .HasPrecision(8, 2);
                    
                entity.Property(s => s.AmbientTemperature)
                    .HasPrecision(8, 2);
                
                entity.HasOne(s => s.Vehicle)
                      .WithMany()
                      .HasForeignKey(s => s.VehicleId)
                      .OnDelete(DeleteBehavior.Cascade);
                
                entity.HasIndex(s => new { s.VehicleId, s.Timestamp })
                    .IsDescending(false, true);
                    
                entity.HasIndex(s => s.Timestamp)
                    .IsDescending();
                    
                entity.HasIndex(s => s.VehicleId);
                
                entity.HasIndex(s => new { s.VehicleId, s.FuelLevel, s.Timestamp });
            });
        }
    }
}
