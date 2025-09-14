using Domain.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Infrastructure;
using System.Threading;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace Application.Abstractions.Data
{
    public interface IApplicationDbContext
    {
        DbSet<User> Users { get; set; }
        DbSet<Vehicle> Vehicles { get; set; }
        DbSet<SensorData> SensorData { get; set; }
        DbSet<Geofence> Geofences { get; set; }
        
        Task<int> SaveChangesAsync(CancellationToken cancellationToken);
        int SaveChanges();
        DatabaseFacade Database { get; }
        EntityEntry Entry(object entity);
    }
}

