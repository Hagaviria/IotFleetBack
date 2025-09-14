# 🚀 BACKEND_SETUP.md - Guía de Despliegue del Backend

## 📋 Resumen

Esta guía te permitirá configurar y ejecutar el **Backend del IoT Fleet Management System** en tu entorno local. El backend está desarrollado en .NET 9.0 con PostgreSQL como base de datos.

---

## 🎯 Requisitos del Sistema

### Software Requerido

#### **Backend (.NET)**
- **.NET 9.0 SDK** o superior
- **Visual Studio 2022** (recomendado) o **VS Code**
- **Git** para control de versiones

#### **Base de Datos**
- **PostgreSQL 15.x** o superior
- **pgAdmin** (opcional, para gestión visual)

#### **Herramientas Adicionales**
- **Docker Desktop** (opcional, para containerización)
- **Postman** o **Insomnia** (para testing de API)

### Verificación de Requisitos

```bash
# Verificar .NET
dotnet --version
# Debe mostrar: 9.0.x o superior

# Verificar PostgreSQL
psql --version
# Debe mostrar: psql (PostgreSQL) 15.x o superior

# Verificar Git
git --version
```

---

## 📥 Instalación de Dependencias

### 1. **Instalar .NET 9.0 SDK**

#### Windows
```powershell
# Descargar desde: https://dotnet.microsoft.com/download/dotnet/9.0
# O usar winget
winget install Microsoft.DotNet.SDK.9
```

#### macOS
```bash
# Usar Homebrew
brew install --cask dotnet-sdk
```

#### Linux (Ubuntu/Debian)
```bash
# Agregar repositorio Microsoft
wget https://packages.microsoft.com/config/ubuntu/22.04/packages-microsoft-prod.deb -O packages-microsoft-prod.deb
sudo dpkg -i packages-microsoft-prod.deb
rm packages-microsoft-prod.deb

# Instalar .NET SDK
sudo apt-get update
sudo apt-get install -y dotnet-sdk-9.0
```

### 2. **Instalar PostgreSQL**

#### Windows
```powershell
# Descargar desde: https://www.postgresql.org/download/windows/
# O usar Chocolatey
choco install postgresql
```

#### macOS
```bash
# Usar Homebrew
brew install postgresql@15
brew services start postgresql@15
```

#### Linux (Ubuntu/Debian)
```bash
# Instalar PostgreSQL
sudo apt update
sudo apt install postgresql postgresql-contrib

# Iniciar servicio
sudo systemctl start postgresql
sudo systemctl enable postgresql
```

---

## 🗄️ Configuración de Base de Datos

### 1. **Configurar PostgreSQL**

```bash
# Conectar a PostgreSQL como superusuario
sudo -u postgres psql

# Crear usuario y base de datos
CREATE USER iotfleet_user WITH PASSWORD 'iotfleet_password';
CREATE DATABASE iotfleet_db OWNER iotfleet_user;
GRANT ALL PRIVILEGES ON DATABASE iotfleet_db TO iotfleet_user;

# Salir
\q
```

### 2. **Verificar Conexión**

```bash
# Probar conexión
psql -h localhost -U iotfleet_user -d iotfleet_db
# Ingresar password: iotfleet_password
```

---

## 📁 Clonación del Repositorio

```bash
# Clonar el repositorio del backend
git clone https://github.com/tu-usuario/iot-fleet-backend.git
cd iot-fleet-backend

# Verificar estructura
ls -la
# Debe mostrar: IotFleet/, src/, BACKEND_DESIGN.md, BACKEND_SETUP.md
```

---

## ⚙️ Configuración del Backend

### 1. **Configurar Variables de Entorno**

Crear archivo `IotFleet/IotFleet/appsettings.Development.json`:

```json
{
  "ConnectionStrings": {
    "myappdb": "Host=localhost;Port=5432;Database=iotfleet_db;Username=iotfleet_user;Password=iotfleet_password"
  },
  "Jwt": {
    "Secret": "e1881dd339324f54f78132a1908c570cb0b37d536269a6915d54edc2c857c0ba",
    "Issuer": "IotFleet",
    "Audience": "IotFleetUsers",
    "ExpirationInMinutes": 720
  },
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft.AspNetCore": "Warning",
      "Microsoft.EntityFrameworkCore": "Information"
    }
  },
  "Serilog": {
    "Using": [ "Serilog.Sinks.Console", "Serilog.Sinks.File" ],
    "MinimumLevel": "Information",
    "WriteTo": [
      {
        "Name": "Console",
        "Args": {
          "outputTemplate": "[{Timestamp:HH:mm:ss} {Level:u3}] {Message:lj}{NewLine}{Exception}"
        }
      },
      {
        "Name": "File",
        "Args": {
          "path": "Logs/app-log-.txt",
          "rollingInterval": "Day",
          "retainedFileCountLimit": 7,
          "outputTemplate": "[{Timestamp:yyyy-MM-dd HH:mm:ss} {Level:u3}] {Message:lj}{NewLine}{Exception}"
        }
      }
    ],
    "Enrich": [ "FromLogContext", "WithMachineName", "WithThreadId" ],
    "Properties": {
      "Application": "IoT Fleet Management API"
    }
  }
}
```

### 2. **Restaurar Dependencias**

```bash
# Navegar al directorio del backend
cd IotFleet

# Restaurar paquetes NuGet
dotnet restore

# Verificar que no hay errores
dotnet build
```

### 3. **Ejecutar Migraciones**

```bash
# Aplicar migraciones a la base de datos
dotnet ef database update --project IotFleet --startup-project IotFleet

# Verificar que las tablas se crearon
psql -h localhost -U iotfleet_user -d iotfleet_db -c "\dt"
```

### 4. **Poblar Datos de Prueba**

```bash
# Ejecutar el backend
dotnet run --project IotFleet

# En otra terminal, poblar datos de prueba
curl -X POST "https://localhost:7162/api/SeedData/populate" \
  -H "accept: application/json" \
  -k
```

### 5. **Verificar Backend**

```bash
# El backend debe estar ejecutándose en:
# HTTP:  http://localhost:5000
# HTTPS: https://localhost:7162

# Verificar health check
curl -k https://localhost:7162/health

# Verificar Swagger
# Abrir en navegador: https://localhost:7162/swagger
```

---

## 🧪 Ejecutar Tests

### Backend Tests

```bash
# Navegar al directorio de tests
cd src/Infrastructure.Tests

# Ejecutar todos los tests
dotnet test

# Ejecutar con cobertura
dotnet test --collect:"XPlat Code Coverage"

# Ejecutar con salida detallada
dotnet test --logger "console;verbosity=detailed"
```

### Scripts de Testing

```bash
# Windows
.\run-tests.ps1

# Linux/macOS
./run-tests.sh
```

---

## 🐳 Configuración con Docker (Opcional)

### 1. **Docker Compose para Base de Datos**

Crear archivo `docker-compose.db.yml`:

```yaml
version: '3.8'
services:
  postgres:
    image: postgres:15
    container_name: iotfleet-postgres
    environment:
      POSTGRES_DB: iotfleet_db
      POSTGRES_USER: iotfleet_user
      POSTGRES_PASSWORD: iotfleet_password
    ports:
      - "5432:5432"
    volumes:
      - postgres_data:/var/lib/postgresql/data
    restart: unless-stopped

volumes:
  postgres_data:
```

### 2. **Ejecutar Base de Datos con Docker**

```bash
# Ejecutar solo la base de datos
docker-compose -f docker-compose.db.yml up -d

# Verificar que está ejecutándose
docker ps

# Conectar a la base de datos
docker exec -it iotfleet-postgres psql -U iotfleet_user -d iotfleet_db
```

---

## 🚀 Scripts de Automatización

### 1. **Script de Inicio (Windows)**

Crear archivo `start-backend.ps1`:

```powershell
# start-backend.ps1 - Script para iniciar el Backend IoT Fleet
Write-Host "🔧 Iniciando IoT Fleet Backend..." -ForegroundColor Green

# Verificar que estamos en el directorio correcto
if (-not (Test-Path "IotFleet")) {
    Write-Host "❌ Error: No se encontró el directorio 'IotFleet'" -ForegroundColor Red
    Write-Host "💡 Asegúrate de ejecutar este script desde el directorio del repositorio backend" -ForegroundColor Yellow
    exit 1
}

# Navegar al directorio del proyecto
cd IotFleet

# Verificar que .NET está instalado
try {
    $dotnetVersion = dotnet --version
    Write-Host "✅ .NET versión: $dotnetVersion" -ForegroundColor Green
} catch {
    Write-Host "❌ Error: .NET no está instalado o no está en el PATH" -ForegroundColor Red
    exit 1
}

# Restaurar dependencias
Write-Host "📦 Restaurando dependencias..." -ForegroundColor Yellow
dotnet restore

# Compilar proyecto
Write-Host "🔨 Compilando proyecto..." -ForegroundColor Yellow
dotnet build

# Verificar que PostgreSQL está ejecutándose
Write-Host "📊 Verificando PostgreSQL..." -ForegroundColor Yellow

# Aplicar migraciones
Write-Host "🗄️ Aplicando migraciones..." -ForegroundColor Yellow
dotnet ef database update

# Iniciar el backend
Write-Host "🚀 Iniciando backend..." -ForegroundColor Green
Write-Host "🌐 URL: https://localhost:7162" -ForegroundColor Cyan
Write-Host "📚 Swagger: https://localhost:7162/swagger" -ForegroundColor Cyan
Write-Host "💡 Para detener, presiona Ctrl+C" -ForegroundColor Yellow

dotnet run
```

### 2. **Script de Inicio (Linux/macOS)**

Crear archivo `start-backend.sh`:

```bash
#!/bin/bash
# start-backend.sh - Script para iniciar el Backend IoT Fleet

echo "🔧 Iniciando IoT Fleet Backend..."

# Verificar que estamos en el directorio correcto
if [ ! -d "IotFleet" ]; then
    echo "❌ Error: No se encontró el directorio 'IotFleet'"
    echo "💡 Asegúrate de ejecutar este script desde el directorio del repositorio backend"
    exit 1
fi

# Navegar al directorio del proyecto
cd IotFleet

# Verificar que .NET está instalado
if ! command -v dotnet &> /dev/null; then
    echo "❌ Error: .NET no está instalado o no está en el PATH"
    exit 1
fi

echo "✅ .NET versión: $(dotnet --version)"

# Restaurar dependencias
echo "📦 Restaurando dependencias..."
dotnet restore

# Compilar proyecto
echo "🔨 Compilando proyecto..."
dotnet build

# Verificar que PostgreSQL está ejecutándose
echo "📊 Verificando PostgreSQL..."

# Aplicar migraciones
echo "🗄️ Aplicando migraciones..."
dotnet ef database update

# Iniciar el backend
echo "🚀 Iniciando backend..."
echo "🌐 URL: https://localhost:7162"
echo "📚 Swagger: https://localhost:7162/swagger"
echo "💡 Para detener, presiona Ctrl+C"

dotnet run
```

```bash
# Hacer ejecutable
chmod +x start-backend.sh
```

---

## 🔍 Verificación del Sistema

### 1. **Checklist de Verificación**

#### Backend
- [ ] ✅ .NET 9.0 SDK instalado
- [ ] ✅ PostgreSQL ejecutándose
- [ ] ✅ Migraciones aplicadas
- [ ] ✅ Backend ejecutándose en https://localhost:7162
- [ ] ✅ Swagger accesible
- [ ] ✅ Health check responde
- [ ] ✅ Datos de prueba cargados

#### Tests
- [ ] ✅ Tests del backend pasan (26/30)
- [ ] ✅ Cobertura de código > 80%

### 2. **Comandos de Verificación**

```bash
# Verificar backend
curl -k https://localhost:7162/health
curl -k https://localhost:7162/api/vehicles

# Verificar base de datos
psql -h localhost -U iotfleet_user -d iotfleet_db -c "SELECT COUNT(*) FROM \"Vehicles\";"

# Verificar tests
cd src/Infrastructure.Tests && dotnet test
```

---

## 🐛 Troubleshooting

### Problemas Comunes

#### 1. **Error de Conexión a Base de Datos**

```bash
# Verificar que PostgreSQL está ejecutándose
sudo systemctl status postgresql  # Linux
brew services list | grep postgres  # macOS

# Verificar conexión
psql -h localhost -U iotfleet_user -d iotfleet_db

# Recrear base de datos si es necesario
dropdb -U postgres iotfleet_db
createdb -U postgres -O iotfleet_user iotfleet_db
```

#### 2. **Error de Certificados SSL**

```bash
# Configurar certificados de desarrollo
dotnet dev-certs https --trust
```

#### 3. **Error de Puertos en Uso**

```bash
# Verificar puertos en uso
netstat -tulpn | grep :7162  # Backend
netstat -tulpn | grep :5432  # PostgreSQL

# Matar procesos si es necesario
sudo kill -9 $(lsof -t -i:7162)
```

#### 4. **Error de Dependencias**

```bash
# Limpiar cache de .NET
dotnet nuget locals all --clear
dotnet clean
dotnet restore
```

#### 5. **Error de Migraciones**

```bash
# Recrear migraciones
cd IotFleet
dotnet ef migrations remove --project IotFleet
dotnet ef migrations add InitialCreate --project IotFleet
dotnet ef database update --project IotFleet
```

### Logs y Debugging

#### Backend Logs
```bash
# Ver logs en tiempo real
tail -f IotFleet/Logs/app-log-*.txt

# Logs de consola
cd IotFleet && dotnet run --verbosity detailed
```

---

## 📚 Recursos Adicionales

### Documentación
- [.NET 9.0 Documentation](https://docs.microsoft.com/en-us/dotnet/)
- [PostgreSQL Documentation](https://www.postgresql.org/docs/)
- [Entity Framework Core](https://docs.microsoft.com/en-us/ef/core/)
- [SignalR Documentation](https://docs.microsoft.com/en-us/aspnet/core/signalr/)

### Herramientas de Desarrollo
- [Postman](https://www.postman.com/) - Testing de API
- [pgAdmin](https://www.pgadmin.org/) - Gestión de PostgreSQL
- [DBeaver](https://dbeaver.io/) - Cliente universal de base de datos

### Comunidad
- [Stack Overflow](https://stackoverflow.com/questions/tagged/.net)
- [GitHub Issues](https://github.com/tu-usuario/iot-fleet-backend/issues)
- [Discord .NET](https://discord.gg/dotnet)

---

## 🎉 ¡Listo para Desarrollar!

Una vez completados todos los pasos, deberías tener:

- ✅ **Backend** ejecutándose en https://localhost:7162
- ✅ **Base de datos** PostgreSQL configurada y poblada
- ✅ **Tests** ejecutándose correctamente
- ✅ **Documentación** completa disponible

### Próximos Pasos

1. **Explorar la API** en Swagger: https://localhost:7162/swagger
2. **Probar los endpoints** con Postman
3. **Revisar los tests** para entender la funcionalidad
4. **Leer BACKEND_DESIGN.md** para entender la arquitectura
5. **Contribuir al proyecto** siguiendo las mejores prácticas

### Credenciales de Prueba

- **Usuario Admin**: `admin` / `12345`
- **Usuario Regular**: `user` / `12345`

---

*Guía creada: Enero 2024*  
*Versión: 1.0*  
*Para soporte: [Crear un issue](https://github.com/tu-usuario/iot-fleet-backend/issues)*
