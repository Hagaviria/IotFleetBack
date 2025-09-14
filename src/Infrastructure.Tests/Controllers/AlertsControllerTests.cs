using System;
using System.Threading;
using System.Threading.Tasks;
using Application.Abstractions.Data;
using Application.Abstractions.Services;
using Domain.DTOs;
using Domain.Models;
using FluentAssertions;
using IotFleet.Controllers;
using Microsoft.AspNetCore.Mvc;
using Moq;
using Xunit;

namespace Infrastructure.Tests.Controllers;

public class AlertsControllerTests
{
    private readonly Mock<IApplicationDbContext> _mockContext;
    private readonly Mock<IFuelPredictionService> _mockFuelPredictionService;
    private readonly Mock<IWebSocketService> _mockWebSocketService;
    private readonly AlertsController _controller;

    public AlertsControllerTests()
    {
        _mockContext = new Mock<IApplicationDbContext>();
        _mockFuelPredictionService = new Mock<IFuelPredictionService>();
        _mockWebSocketService = new Mock<IWebSocketService>();
        
        _controller = new AlertsController(
            _mockContext.Object,
            _mockFuelPredictionService.Object,
            _mockWebSocketService.Object);
    }

    [Fact]
    public async Task CalculateFuelAlert_WithValidVehicleId_ShouldReturnFuelAlert()
    {
        // Arrange
        var vehicleId = Guid.NewGuid().ToString();
        var vehicle = new Vehicle
        {
            Id = Guid.Parse(vehicleId),
            LicensePlate = "ABC-123",
            FuelCapacity = 50.0,
            AverageConsumption = 10.0
        };

        var sensorData = new SensorData
        {
            VehicleId = vehicle.Id,
            FuelLevel = 5.0,
            Speed = 60.0,
            FuelConsumption = 12.0,
            EngineTemperature = 85.0,
            Timestamp = DateTime.UtcNow
        };

        var expectedAlert = new FuelAlertDTO(
            vehicle.Id,
            vehicle.LicensePlate,
            sensorData.FuelLevel,
            0.5,
            DateTime.UtcNow,
            "CRITICAL",
            "Test alert message",
            30.0
        );

        _mockFuelPredictionService
            .Setup(x => x.CalculateFuelAutonomy(It.IsAny<Vehicle>(), It.IsAny<SensorData>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedAlert);

        // Act
        var result = await _controller.CalculateFuelAlert(vehicleId);

        // Assert
        result.Should().BeOfType<OkObjectResult>();
        var okResult = result as OkObjectResult;
        okResult!.Value.Should().Be(expectedAlert);
        
        _mockFuelPredictionService.Verify(
            x => x.CalculateFuelAutonomy(It.IsAny<Vehicle>(), It.IsAny<SensorData>(), It.IsAny<CancellationToken>()),
            Times.Once);
        
        _mockWebSocketService.Verify(
            x => x.BroadcastAlert(expectedAlert),
            Times.Once);
    }

    [Fact]
    public async Task CalculateFuelAlert_WithInvalidVehicleId_ShouldReturnBadRequest()
    {
        // Arrange
        var invalidVehicleId = "invalid-guid";

        // Act
        var result = await _controller.CalculateFuelAlert(invalidVehicleId);

        // Assert
        result.Should().BeOfType<BadRequestObjectResult>();
        
        _mockFuelPredictionService.Verify(
            x => x.CalculateFuelAutonomy(It.IsAny<Vehicle>(), It.IsAny<SensorData>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task CalculateFuelAlert_WithNoFuelAlert_ShouldReturnNoContent()
    {
        // Arrange
        var vehicleId = Guid.NewGuid().ToString();
        
        _mockFuelPredictionService
            .Setup(x => x.CalculateFuelAutonomy(It.IsAny<Vehicle>(), It.IsAny<SensorData>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((FuelAlertDTO?)null);

        // Act
        var result = await _controller.CalculateFuelAlert(vehicleId);

        // Assert
        result.Should().BeOfType<NoContentResult>();
        
        _mockWebSocketService.Verify(
            x => x.BroadcastAlert(It.IsAny<FuelAlertDTO>()),
            Times.Never);
    }

    [Fact]
    public async Task CalculateFuelAlert_WithException_ShouldReturnInternalServerError()
    {
        // Arrange
        var vehicleId = Guid.NewGuid().ToString();
        
        _mockFuelPredictionService
            .Setup(x => x.CalculateFuelAutonomy(It.IsAny<Vehicle>(), It.IsAny<SensorData>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new Exception("Test exception"));

        // Act
        var result = await _controller.CalculateFuelAlert(vehicleId);

        // Assert
        result.Should().BeOfType<ObjectResult>();
        var objectResult = result as ObjectResult;
        objectResult!.StatusCode.Should().Be(500);
    }
}
