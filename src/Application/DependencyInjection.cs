using Application.Abstractions.Catalogs;
using Application.Features.User.Command;
using Application.Features.User.Query;
using Application.Features.SensorData.Command;
using Application.Features.SensorData.Query;
using Application.Features.Vehicle.Command;
using Application.Features.Vehicle.Query;
using FluentValidation;
using Microsoft.Extensions.DependencyInjection;

namespace Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        // Register validators
        services.AddValidatorsFromAssembly(typeof(DependencyInjection).Assembly, includeInternalTypes: true);
        
        // Register User handlers
        services.AddScoped<UserCommandHandler>();
        services.AddScoped<UserQueryHandler>();
        services.AddScoped<UserManagementCommandHandler>();
        services.AddScoped<UserManagementQueryHandler>();
        
        // Register SensorData handlers
        services.AddScoped<SensorDataCommandHandler>();
        services.AddScoped<SensorDataQueryHandler>();
        
        // Register Vehicle handlers
        services.AddScoped<VehicleCommandHandler>();
        services.AddScoped<VehicleQueryHandler>();
        
        
        return services;
    }
}
