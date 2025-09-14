# 🧪 Tests Unitarios - Backend IoT Fleet

Este proyecto contiene los tests unitarios para la lógica crítica del backend de IoT Fleet.

## 📋 Tests Implementados

### 1. **FuelPredictionServiceTests**
- ✅ Cálculo de autonomía de combustible con diferentes niveles
- ✅ Determinación de severidad de alertas (CRITICAL, HIGH, MEDIUM, LOW)
- ✅ Manejo de temperatura alta del motor
- ✅ Cálculo con consumo cero
- ✅ Manejo de velocidad nula
- ✅ Manejo de excepciones

### 2. **TokenProviderTests**
- ✅ Generación de tokens JWT válidos
- ✅ Mapeo correcto de roles por perfil de usuario
- ✅ Validación de tokens
- ✅ Manejo de tokens expirados
- ✅ Manejo de tokens inválidos
- ✅ Caracteres especiales en nombres de usuario

### 3. **PrivacyMiddlewareTests**
- ✅ Enmascaramiento de datos sensibles para usuarios no admin
- ✅ Preservación de datos originales para usuarios admin
- ✅ Manejo de respuestas JSON válidas e inválidas
- ✅ Procesamiento de arrays y objetos anidados
- ✅ Filtrado por rutas de API

### 4. **VehicleSimulationServiceTests**
- ✅ Inicialización de simulación con vehículos
- ✅ Manejo de casos sin vehículos
- ✅ Control de estado de simulación (start/stop)
- ✅ Verificación de estado de ejecución

## 🚀 Cómo Ejecutar los Tests

### Opción 1: Scripts Automatizados

**En Windows (PowerShell):**
```powershell
.\run-tests.ps1
```

**En Linux/Mac (Bash):**
```bash
./run-tests.sh
```

### Opción 2: Comandos Manuales

```bash
# Navegar al directorio de tests
cd src/Infrastructure.Tests

# Restaurar paquetes
dotnet restore

# Compilar
dotnet build

# Ejecutar tests
dotnet test

# Ejecutar con cobertura
dotnet test --collect:"XPlat Code Coverage"

# Ejecutar con salida detallada
dotnet test --logger "console;verbosity=detailed"
```

### Opción 3: Desde Visual Studio
1. Abrir la solución en Visual Studio
2. Ir a **Test Explorer**
3. Ejecutar todos los tests o tests específicos

## 📊 Cobertura de Código

Los tests están diseñados para cubrir:
- **Lógica de negocio crítica** (cálculo de combustible)
- **Autenticación y autorización** (JWT tokens)
- **Seguridad y privacidad** (enmascaramiento de datos)
- **Servicios de simulación** (gestión de vehículos)

## 🔧 Configuración

El archivo `appsettings.Test.json` contiene la configuración específica para los tests:
- Claves JWT para testing
- Cadena de conexión a base de datos de prueba
- Configuración de logging

## 📝 Estructura de Tests

```
Infrastructure.Tests/
├── Services/
│   ├── FuelPredictionServiceTests.cs
│   └── VehicleSimulationServiceTests.cs
├── Authentication/
│   └── TokenProviderTests.cs
├── Middleware/
│   └── PrivacyMiddlewareTests.cs
├── appsettings.Test.json
└── README.md
```

## 🎯 Criterios de Éxito

Los tests deben pasar todos los casos para considerar la implementación correcta:
- ✅ **100% de tests pasando**
- ✅ **Cobertura de código > 80%** en lógica crítica
- ✅ **Sin warnings** en la compilación
- ✅ **Tiempo de ejecución < 30 segundos**

## 🐛 Troubleshooting

### Error: "Package not found"
```bash
dotnet restore --force
```

### Error: "Build failed"
```bash
dotnet clean
dotnet build --verbosity detailed
```

### Error: "Tests not found"
```bash
dotnet test --list-tests
```

## 📈 Métricas de Calidad

- **Total de Tests**: 25+
- **Cobertura Objetivo**: >80%
- **Tiempo de Ejecución**: <30 segundos
- **Frameworks**: xUnit, Moq, FluentAssertions
