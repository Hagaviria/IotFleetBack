# 🏗️ BACKEND_DESIGN.md - IoT Fleet Management Backend

## 📋 Resumen Ejecutivo

El **Backend del IoT Fleet Management System** es una API REST desarrollada en **.NET 9.0** que implementa **Clean Architecture** con **CQRS** y **Domain-Driven Design (DDD)**. Proporciona servicios de monitoreo en tiempo real, alertas predictivas, y gestión de privacidad de datos para flotas vehiculares.

---

## 🎯 Objetivos del Backend

### Funcionalidades Principales
- **API REST** para gestión de vehículos, usuarios y datos de sensores
- **Comunicación en Tiempo Real** con SignalR para actualizaciones instantáneas
- **Alertas Predictivas** de combustible y mantenimiento
- **Gestión de Privacidad** con enmascaramiento de datos sensibles
- **Autenticación JWT** y autorización basada en roles
- **Simulación de Vehículos** para datos de prueba

### Requisitos No Funcionales
- **Escalabilidad**: Soporte para miles de vehículos simultáneos
- **Rendimiento**: Respuesta API < 100ms
- **Seguridad**: Autenticación JWT y autorización granular
- **Privacidad**: Cumplimiento con regulaciones de protección de datos
- **Disponibilidad**: 99.9% de uptime

---

## 🏛️ Arquitectura del Backend

### Clean Architecture Layers

```
┌─────────────────────────────────────────┐
│              Presentation               │
│           (Controllers, Hubs)           │
├─────────────────────────────────────────┤
│              Application                │
│         (Use Cases, CQRS)              │
├─────────────────────────────────────────┤
│               Domain                    │
│        (Entities, Business Logic)      │
├─────────────────────────────────────────┤
│            Infrastructure               │
│    (Database, External Services)       │
└─────────────────────────────────────────┘
```

### Estructura de Proyectos

```
IotFleet/
├── IotFleet/                    # API Web (Presentation Layer)
│   ├── Controllers/             # Controladores REST
│   ├── Hubs/                   # SignalR Hubs
│   ├── Middleware/             # Middleware personalizado
│   ├── Program.cs              # Punto de entrada
│   └── appsettings.json        # Configuración
├── src/
│   ├── Application/            # Application Layer
│   │   ├── Abstractions/       # Interfaces y contratos
│   │   ├── Features/           # CQRS Handlers
│   │   └── Services/           # Servicios de aplicación
│   ├── Domain/                 # Domain Layer
│   │   ├── Models/             # Entidades de dominio
│   │   ├── DTOs/               # Data Transfer Objects
│   │   └── Errors/             # Errores de dominio
│   ├── Infrastructure/         # Infrastructure Layer
│   │   ├── Authentication/     # JWT, Claims, Roles
│   │   ├── Database/           # EF Core, PostgreSQL
│   │   ├── Hubs/              # SignalR Real-time
│   │   ├── Services/          # External services
│   │   └── Time/              # DateTime abstractions
│   └── SharedKernel/          # Shared utilities
└── src/Infrastructure.Tests/   # Unit Tests
```

---

## 🛠️ Stack Tecnológico

### Framework y Runtime
- **.NET 9.0**: Última versión LTS con mejoras de rendimiento
- **ASP.NET Core**: Framework web moderno y escalable
- **C# 12**: Lenguaje con características avanzadas

### Arquitectura y Patrones
- **Clean Architecture**: Separación clara de responsabilidades
- **CQRS**: Separación de comandos y consultas
- **MediatR**: Patrón Mediator para desacoplamiento
- **Repository Pattern**: Abstracción de acceso a datos
- **Dependency Injection**: Inversión de control nativa

### Base de Datos
- **PostgreSQL**: Base de datos relacional robusta
- **Entity Framework Core 9.0**: ORM con migraciones automáticas
- **Npgsql**: Driver PostgreSQL optimizado

### Comunicación en Tiempo Real
- **SignalR**: WebSockets para comunicación bidireccional
- **FleetHub**: Hub centralizado para eventos de flota

### Autenticación y Seguridad
- **JWT Bearer Tokens**: Autenticación stateless
- **Role-based Authorization**: Control de acceso granular
- **Privacy Middleware**: Enmascaramiento automático de datos

### Logging y Monitoreo
- **Serilog**: Logging estructurado y configurable
- **Health Checks**: Monitoreo de estado del sistema
- **Swagger/OpenAPI**: Documentación automática de API

---



## 🧪 Testing Strategy

### Backend Testing

```csharp
// Unit Tests con xUnit, Moq, FluentAssertions
[Fact]
public async Task CalculateFuelAutonomy_WithLowFuelLevel_ShouldReturnAlert()
{
    // Arrange
    var vehicle = new Vehicle { FuelCapacity = 50.0 };
    var sensorData = new SensorData { FuelLevel = 5.0 };
    
    // Act
    var result = await _service.CalculateFuelAutonomy(vehicle, sensorData);
    
    // Assert
    result.Should().NotBeNull();
    result.Severity.Should().Be("CRITICAL");
}
```

### Cobertura de Tests

- **30 tests unitarios** implementados
- **26 tests pasando** (87% de éxito)
- **Cobertura de lógica crítica**: Cálculo de combustible, autenticación, privacidad

---

## 🚀 Deployment y DevOps

### Containerización

```dockerfile
# Backend Dockerfile
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src
COPY ["IotFleet/IotFleet.csproj", "IotFleet/"]
RUN dotnet restore "IotFleet/IotFleet.csproj"
COPY . .
WORKDIR "/src/IotFleet"
RUN dotnet build "IotFleet.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "IotFleet.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "IotFleet.dll"]
```

### CI/CD Pipeline

```yaml
# GitHub Actions
name: Build and Test Backend
on: [push, pull_request]
jobs:
  test:
    runs-on: ubuntu-latest
    steps:
      - uses: actions/checkout@v3
      - name: Setup .NET
        uses: actions/setup-dotnet@v3
        with:
          dotnet-version: '9.0.x'
      - name: Restore dependencies
        run: dotnet restore
      - name: Build
        run: dotnet build --no-restore
      - name: Test
        run: dotnet test --no-build --verbosity normal
```

---

## 📈 Trade-offs y Decisiones Técnicas

### ✅ Decisiones Acertadas

#### 1. **Clean Architecture**
- **Pro**: Mantenibilidad, testabilidad, independencia de frameworks
- **Contra**: Complejidad inicial, curva de aprendizaje
- **Justificación**: Proyecto a largo plazo que requiere evolución constante

#### 2. **PostgreSQL sobre SQL Server**
- **Pro**: Open source, mejor rendimiento, características avanzadas
- **Contra**: Menos integración con ecosistema Microsoft
- **Justificación**: Costo-efectividad y flexibilidad de deployment

#### 3. **SignalR sobre WebSockets nativos**
- **Pro**: Abstracción de transporte, reconexión automática, escalabilidad
- **Contra**: Dependencia de Microsoft, overhead
- **Justificación**: Simplicidad de implementación y características avanzadas

### ⚠️ Trade-offs Considerados

#### 1. **CQRS Completo vs Simple**
- **Decisión**: CQRS simple (sin Event Sourcing)
- **Razón**: Complejidad vs beneficio no justificada para el dominio actual

#### 2. **Microservicios vs Monolito Modular**
- **Decisión**: Monolito modular con Clean Architecture
- **Razón**: Equipo pequeño, deployment simplificado, evolución gradual

#### 3. **GraphQL vs REST**
- **Decisión**: REST con OpenAPI/Swagger
- **Razón**: Simplicidad, herramientas maduras, equipo familiarizado

---

## 🔮 Evolución Futura

### Roadmap Técnico

#### Fase 1: Optimización (Q1 2024)
- [ ] Implementar caching con Redis
- [ ] Optimizar consultas de base de datos
- [ ] Añadir métricas con Prometheus

#### Fase 2: Escalabilidad (Q2 2024)
- [ ] Migrar a microservicios
- [ ] Implementar Event Sourcing
- [ ] Añadir Message Queues (RabbitMQ)

#### Fase 3: Inteligencia (Q3 2024)
- [ ] Machine Learning para predicciones
- [ ] Análisis de patrones de conducción
- [ ] Optimización de rutas

### Consideraciones de Escalabilidad

#### Horizontal Scaling
```csharp
// Load Balancer Configuration
services.AddSignalR(options =>
{
    options.EnableDetailedErrors = false;
    options.MaximumReceiveMessageSize = 1024 * 1024;
}).AddRedis(connectionString); // Redis backplane
```

#### Database Sharding
```csharp
// Future: Sharding by Fleet ID
public class ShardedDbContext : ApplicationDbContext
{
    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        var shardKey = GetShardKey();
        var connectionString = GetConnectionStringForShard(shardKey);
        optionsBuilder.UseNpgsql(connectionString);
    }
}
```

---

## 📚 Referencias y Estándares

### Patrones y Principios
- **SOLID Principles**: Single Responsibility, Open/Closed, Liskov Substitution, Interface Segregation, Dependency Inversion
- **DRY**: Don't Repeat Yourself
- **KISS**: Keep It Simple, Stupid
- **YAGNI**: You Aren't Gonna Need It

### Estándares de Código
- **C# Coding Standards**: Microsoft .NET Guidelines
- **REST API Design**: RESTful API Design Principles
- **Security**: OWASP Top 10, JWT Best Practices

### Herramientas de Calidad
- **SonarQube**: Análisis de calidad de código
- **xUnit**: Framework de testing
- **Moq**: Framework de mocking
- **FluentAssertions**: Biblioteca de aserciones

---

## 🎯 Conclusión

El **Backend del IoT Fleet Management System** implementa una arquitectura moderna y escalable que balancea complejidad técnica con mantenibilidad a largo plazo. La elección de **Clean Architecture** con **CQRS** proporciona una base sólida para el crecimiento futuro, mientras que el stack tecnológico seleccionado ofrece las herramientas necesarias para implementar funcionalidades avanzadas de IoT y análisis en tiempo real.

La arquitectura modular permite evolución gradual hacia microservicios cuando sea necesario, y la implementación de patrones de seguridad y privacidad asegura cumplimiento con regulaciones actuales y futuras.

---

*Documento creado: Enero 2024*  
*Versión: 1.0*  
*Autor: Equipo de Desarrollo IoT Fleet Backend*
