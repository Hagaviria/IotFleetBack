#!/bin/bash

# Script para ejecutar tests del backend
# Bash script para ejecutar todos los tests unitarios

echo "🧪 Ejecutando Tests Unitarios del Backend IoT Fleet"
echo "================================================="

# Navegar al directorio del proyecto
cd src/Infrastructure.Tests

echo "📁 Directorio actual: $(pwd)"

# Restaurar paquetes NuGet
echo "📦 Restaurando paquetes NuGet..."
dotnet restore

if [ $? -ne 0 ]; then
    echo "❌ Error al restaurar paquetes NuGet"
    exit 1
fi

# Compilar el proyecto
echo "🔨 Compilando proyecto de tests..."
dotnet build

if [ $? -ne 0 ]; then
    echo "❌ Error al compilar el proyecto"
    exit 1
fi

# Ejecutar tests con cobertura
echo "🚀 Ejecutando tests con cobertura de código..."
dotnet test --collect:"XPlat Code Coverage" --logger "console;verbosity=detailed"

if [ $? -eq 0 ]; then
    echo "✅ Todos los tests pasaron exitosamente!"
else
    echo "❌ Algunos tests fallaron"
fi

# Generar reporte de cobertura (opcional)
echo "📊 Generando reporte de cobertura..."
echo "Los archivos de cobertura se encuentran en: TestResults/"

# Volver al directorio raíz
cd ../..

echo "🎉 Proceso completado!"
echo "Para ver resultados detallados, revisa la salida anterior."
