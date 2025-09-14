# Script para ejecutar tests del backend
# PowerShell script para ejecutar todos los tests unitarios

Write-Host "🧪 Ejecutando Tests Unitarios del Backend IoT Fleet" -ForegroundColor Green
Write-Host "=================================================" -ForegroundColor Green

# Navegar al directorio del proyecto
Set-Location "src/Infrastructure.Tests"

Write-Host "📁 Directorio actual: $(Get-Location)" -ForegroundColor Yellow

# Restaurar paquetes NuGet
Write-Host "📦 Restaurando paquetes NuGet..." -ForegroundColor Cyan
dotnet restore

if ($LASTEXITCODE -ne 0) {
    Write-Host "❌ Error al restaurar paquetes NuGet" -ForegroundColor Red
    exit 1
}

# Compilar el proyecto
Write-Host "🔨 Compilando proyecto de tests..." -ForegroundColor Cyan
dotnet build

if ($LASTEXITCODE -ne 0) {
    Write-Host "❌ Error al compilar el proyecto" -ForegroundColor Red
    exit 1
}

# Ejecutar tests con cobertura
Write-Host "🚀 Ejecutando tests con cobertura de código..." -ForegroundColor Cyan
dotnet test --collect:"XPlat Code Coverage" --logger "console;verbosity=detailed"

if ($LASTEXITCODE -eq 0) {
    Write-Host "✅ Todos los tests pasaron exitosamente!" -ForegroundColor Green
} else {
    Write-Host "❌ Algunos tests fallaron" -ForegroundColor Red
}

# Generar reporte de cobertura (opcional)
Write-Host "📊 Generando reporte de cobertura..." -ForegroundColor Cyan
Write-Host "Los archivos de cobertura se encuentran en: TestResults/" -ForegroundColor Yellow

# Volver al directorio raíz
Set-Location "../.."

Write-Host "🎉 Proceso completado!" -ForegroundColor Green
Write-Host "Para ver resultados detallados, revisa la salida anterior." -ForegroundColor Yellow
