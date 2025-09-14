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
- **Docker Desktop**
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



### 4. **Verificar Backend**

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



---

1. **Explorar la API** en Swagger: https://localhost:7162/swagger
2. **Probar los endpoints** con Postman
3. **Revisar los tests** para entender la funcionalidad
4. **Leer BACKEND_DESIGN.md** para entender la arquitectura
5. **Contribuir al proyecto** siguiendo las mejores prácticas

### Credenciales de Prueba

- **Usuario Admin**: "Correo": "admin@iotfleet.com",
  "Contraseña": "Admin123!",
- **Usuario Regular**: carlos.rodriguez@empresa.com/
Operador123!

---
