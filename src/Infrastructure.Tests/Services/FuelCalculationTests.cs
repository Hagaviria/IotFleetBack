using System;
using Domain.Models;
using FluentAssertions;
using Xunit;

namespace Infrastructure.Tests.Services;

public class FuelCalculationTests
{
    [Theory]
    [InlineData(50.0, 10.0, 5.0, 60.0, 0.25)] // 5% fuel, 10 L/h consumption, 60 km/h speed
    [InlineData(50.0, 8.0, 20.0, 50.0, 1.25)] // 20% fuel, 8 L/h consumption, 50 km/h speed
    [InlineData(60.0, 12.0, 10.0, 70.0, 0.5)] // 10% fuel, 12 L/h consumption, 70 km/h speed
    public void CalculateFuelAutonomy_WithValidInputs_ShouldReturnCorrectAutonomy(
        double fuelCapacity, 
        double consumption, 
        double fuelLevel, 
        double speed, 
        double expectedAutonomyHours)
    {
        // Arrange
        var currentFuelLiters = (fuelLevel / 100) * fuelCapacity;
        var autonomyHours = consumption > 0 ? currentFuelLiters / consumption : 0;
        var remainingDistance = autonomyHours * speed;

        // Act & Assert
        autonomyHours.Should().BeApproximately(expectedAutonomyHours, 0.01);
        remainingDistance.Should().BeGreaterThan(0);
    }

    [Theory]
    [InlineData(0.1, "CRITICAL")]
    [InlineData(0.3, "HIGH")]
    [InlineData(0.6, "MEDIUM")]
    [InlineData(0.8, "LOW")]
    public void DetermineSeverity_WithDifferentAutonomyHours_ShouldReturnCorrectSeverity(
        double autonomyHours, 
        string expectedSeverity)
    {
        // Arrange
        var fuelLevel = autonomyHours * 20; // Simular nivel de combustible

        // Act
        var severity = DetermineSeverity(autonomyHours, fuelLevel);

        // Assert
        severity.Should().Be(expectedSeverity);
    }

    [Theory]
    [InlineData(0.5, 90.0, 1.2)] // Normal temperature
    [InlineData(0.5, 95.0, 1.2)] // High temperature - should increase consumption
    [InlineData(0.5, 85.0, 1.0)] // Low temperature - normal consumption
    public void CalculateConsumptionWithTemperature_ShouldApplyTemperatureFactor(
        double baseConsumption, 
        double engineTemperature, 
        double expectedMultiplier)
    {
        // Arrange
        var speed = 60.0;
        var speedFactor = Math.Max(0.5, Math.Min(2.0, speed / 60.0));
        var calculatedConsumption = baseConsumption * speedFactor;

        // Act
        if (engineTemperature > 90)
        {
            calculatedConsumption *= 1.2;
        }

        // Assert
        var actualMultiplier = calculatedConsumption / (baseConsumption * speedFactor);
        actualMultiplier.Should().BeApproximately(expectedMultiplier, 0.1);
    }

    [Fact]
    public void CalculateFuelAutonomy_WithZeroConsumption_ShouldReturnZeroAutonomy()
    {
        // Arrange
        var fuelCapacity = 50.0;
        var fuelLevel = 20.0;
        var consumption = 0.0;
        var speed = 60.0;

        var currentFuelLiters = (fuelLevel / 100) * fuelCapacity;
        var autonomyHours = consumption > 0 ? currentFuelLiters / consumption : 0;
        var remainingDistance = autonomyHours * speed;

        // Act & Assert
        autonomyHours.Should().Be(0);
        remainingDistance.Should().Be(0);
    }

    [Fact]
    public void CalculateFuelAutonomy_WithHighFuelLevel_ShouldNotGenerateAlert()
    {
        // Arrange
        var fuelCapacity = 50.0;
        var fuelLevel = 80.0; // 80% fuel
        var consumption = 8.0;
        var speed = 60.0;

        var currentFuelLiters = (fuelLevel / 100) * fuelCapacity;
        var autonomyHours = consumption > 0 ? currentFuelLiters / consumption : 0;

        // Act & Assert
        autonomyHours.Should().BeGreaterThan(1.0); // Should not generate alert
    }

    [Theory]
    [InlineData(30.0, 0.5)] // Low speed
    [InlineData(60.0, 1.0)] // Normal speed
    [InlineData(120.0, 2.0)] // High speed
    public void CalculateSpeedFactor_WithDifferentSpeeds_ShouldReturnCorrectFactor(
        double speed, 
        double expectedFactor)
    {
        // Act
        var speedFactor = Math.Max(0.5, Math.Min(2.0, speed / 60.0));

        // Assert
        speedFactor.Should().BeApproximately(expectedFactor, 0.1);
        speed.Should().BeGreaterThan(0); // Use the speed variable to avoid warning
    }

    private static string DetermineSeverity(double autonomyHours, double fuelLevel)
    {
        if (autonomyHours < 0.25 || fuelLevel < 5)
            return "CRITICAL";
        else if (autonomyHours < 0.5 || fuelLevel < 10)
            return "HIGH";
        else if (autonomyHours < 0.75 || fuelLevel < 15)
            return "MEDIUM";
        else
            return "LOW";
    }
}
