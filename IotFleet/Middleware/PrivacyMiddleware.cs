using System.Text.Json;
using Microsoft.AspNetCore.Authorization;

namespace IotFleet.Middleware;

public class PrivacyMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<PrivacyMiddleware> _logger;

    public PrivacyMiddleware(RequestDelegate next, ILogger<PrivacyMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        // Solo aplicar enmascaramiento a respuestas JSON de la API
        if (context.Request.Path.StartsWithSegments("/api") && 
            context.Request.Headers.Accept.Any(h => h?.Contains("application/json") == true))
        {
            var originalBodyStream = context.Response.Body;
            
            var responseBody = new MemoryStream();
            context.Response.Body = responseBody;

            try
            {
                await _next(context);

                // Solo procesar respuestas exitosas con contenido JSON
                if (context.Response.StatusCode == 200 && 
                    context.Response.ContentType?.Contains("application/json") == true)
                {
                    var responseBodyText = await GetResponseBodyText(responseBody);
                    
                    // Verificar si el usuario es admin
                    var isAdmin = context.User?.IsInRole("Admin") == true;
                    
                    if (!isAdmin && !string.IsNullOrEmpty(responseBodyText))
                    {
                        var maskedResponse = MaskSensitiveData(responseBodyText);
                        
                        // Escribir la respuesta enmascarada
                        var bytes = System.Text.Encoding.UTF8.GetBytes(maskedResponse);
                        context.Response.Body = originalBodyStream;
                        context.Response.ContentLength = bytes.Length;
                        await context.Response.Body.WriteAsync(bytes, 0, bytes.Length);
                        return;
                    }
                }

                // Restaurar el stream original y copiar la respuesta
                responseBody.Seek(0, SeekOrigin.Begin);
                await responseBody.CopyToAsync(originalBodyStream);
                context.Response.Body = originalBodyStream;
            }
            finally
            {
                responseBody.Dispose();
            }
        }
        else
        {
            await _next(context);
        }
    }

    private async Task<string> GetResponseBodyText(MemoryStream responseBody)
    {
        responseBody.Seek(0, SeekOrigin.Begin);
        using var reader = new StreamReader(responseBody, leaveOpen: true);
        return await reader.ReadToEndAsync();
    }

    private string MaskSensitiveData(string jsonResponse)
    {
        try
        {
            using var document = JsonDocument.Parse(jsonResponse);
            var maskedDocument = MaskJsonDocument(document);
            
            var stream = new MemoryStream();
            using (var writer = new Utf8JsonWriter(stream, new JsonWriterOptions { Indented = true }))
            {
                maskedDocument.WriteTo(writer);
                writer.Flush();
            }
            
            var result = System.Text.Encoding.UTF8.GetString(stream.ToArray());
            stream.Dispose();
            return result;
        }
        catch (JsonException ex)
        {
            _logger.LogWarning(ex, "Error parsing JSON response for masking");
            return jsonResponse; // Retornar respuesta original si hay error
        }
    }

    private JsonDocument MaskJsonDocument(JsonDocument document)
    {
        var root = document.RootElement;
        var maskedRoot = MaskJsonElement(root);
        
        var stream = new MemoryStream();
        using (var writer = new Utf8JsonWriter(stream))
        {
            maskedRoot.WriteTo(writer);
            writer.Flush();
        }
        
        var result = JsonDocument.Parse(stream.ToArray());
        stream.Dispose();
        return result;
    }

    private JsonElement MaskJsonElement(JsonElement element)
    {
        switch (element.ValueKind)
        {
            case JsonValueKind.Object:
                return MaskJsonObject(element);
            case JsonValueKind.Array:
                return MaskJsonArray(element);
            default:
                return element;
        }
    }

    private JsonElement MaskJsonObject(JsonElement obj)
    {
        var maskedProperties = new Dictionary<string, object>();
        
        foreach (var property in obj.EnumerateObject())
        {
            var key = property.Name;
            var value = property.Value;
            
            // Enmascarar IDs sensibles
            if (ShouldMaskProperty(key))
            {
                maskedProperties[key] = MaskId(value.GetString() ?? "");
            }
            else if (value.ValueKind == JsonValueKind.Object || value.ValueKind == JsonValueKind.Array)
            {
                maskedProperties[key] = MaskJsonElement(value);
            }
            else
            {
                maskedProperties[key] = value;
            }
        }
        
        var jsonString = JsonSerializer.Serialize(maskedProperties);
        using var document = JsonDocument.Parse(jsonString);
        return document.RootElement.Clone();
    }

    private JsonElement MaskJsonArray(JsonElement array)
    {
        var maskedItems = new List<object>();
        
        foreach (var item in array.EnumerateArray())
        {
            if (item.ValueKind == JsonValueKind.Object || item.ValueKind == JsonValueKind.Array)
            {
                maskedItems.Add(MaskJsonElement(item));
            }
            else
            {
                maskedItems.Add(item);
            }
        }
        
        var jsonString = JsonSerializer.Serialize(maskedItems);
        using var document = JsonDocument.Parse(jsonString);
        return document.RootElement.Clone();
    }

    private bool ShouldMaskProperty(string propertyName)
    {
        var sensitiveProperties = new[]
        {
            "Id", "VehicleId", "DeviceId", "UserId", "FleetId",
            "SensorId", "AlertId", "LocationId", "RouteId"
        };
        
        return sensitiveProperties.Any(prop => 
            string.Equals(propertyName, prop, StringComparison.OrdinalIgnoreCase));
    }

    private string MaskId(string id)
    {
        if (string.IsNullOrEmpty(id) || id.Length < 8)
        {
            return "DEV-****-****";
        }

        // Extraer partes del ID (asumiendo formato UUID o similar)
        var parts = id.Split('-');
        
        if (parts.Length >= 3)
        {
            // Formato: XXXXXXXX-XXXX-XXXX-XXXX-XXXXXXXXXXXX
            // Resultado: DEV-****-XXXX (últimos 4 caracteres)
            var lastPart = parts[parts.Length - 1];
            var maskedPart = lastPart.Length >= 4 
                ? lastPart.Substring(lastPart.Length - 4)
                : lastPart;
            return $"DEV-****-{maskedPart.ToUpper()}";
        }
        else
        {
            // Para IDs sin formato estándar, tomar últimos 4 caracteres
            var maskedPart = id.Length >= 4 
                ? id.Substring(id.Length - 4)
                : id;
            return $"DEV-****-{maskedPart.ToUpper()}";
        }
    }
}
