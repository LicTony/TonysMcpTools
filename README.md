# TonysMcpTools

Servidor MCP (Model Context Protocol) desarrollado en C# / .NET 9, diseñado para exponer herramientas de productividad a asistentes de inteligencia artificial compatibles con MCP (por ejemplo, Claude, Gemini, etc.).

---

## ✨ Características

| Herramienta | Descripción |
|---|---|
| `GetCurrentDate` | Retorna la fecha actual en formato `yyyyMMdd`. |
| `GetCurrentDateTime` | Retorna la fecha y hora actual en formato `yyyyMMdd_HHmmss`. |
| `GetCurrentTonysMcpToolsVersion` | Retorna la versión actual del servidor. |
| `ObtenerDetalleResumidoIssueAsync` | Obtiene un resumen ejecutivo de un issue de Jira (clave, estado, prioridad, comentarios, adjuntos). |
| `ObtenerDetalleIssueAsync` | Obtiene el detalle completo de un issue de Jira, incluyendo custom fields. |
| `ObtenerIssueByJqlAsync` | Busca issues de Jira mediante una consulta JQL. |
| `ObtenerWorkLogsAsync` | Obtiene el registro de trabajo (work logs) de un issue de Jira. |

---

## 🚀 Requisitos

- [.NET 9 SDK](https://dotnet.microsoft.com/download/dotnet/9)
- Acceso a una instancia de **Jira Cloud** (usuario + API token)

---

## ⚙️ Configuración

El servidor utiliza variables de entorno para la autenticación con Jira. Los valores se cifran con **Windows DPAPI** (`System.Security.Cryptography.ProtectedData`).

| Variable de entorno | Descripción |
|---|---|
| `MCP_Entropia` | Entropía adicional (salt) usada para el cifrado DPAPI. |
| `MCP_UsuarioJira` | E-mail del usuario de Jira (cifrado con DPAPI). |
| `MCP_TokenJira` | API Token de Jira (cifrado con DPAPI). |
| `MCP_JiraBaseUrl` | URL base de la instancia Jira, ej: `https://miempresa.atlassian.net` (cifrado con DPAPI). |

> **Nota:** Los valores de `MCP_UsuarioJira`, `MCP_TokenJira` y `MCP_JiraBaseUrl` deben estar cifrados previamente con `DpapiHelper.Cifrar()` usando la misma entropía definida en `MCP_Entropia`.

---

## 🏗️ Estructura del proyecto

```
TonysMcpToolsSolution/
├── TonysMcpTools/                  # Proyecto principal (servidor MCP)
│   ├── ApiJiraTools.cs             # Herramientas de integración con Jira
│   ├── DateTimeTools.cs            # Herramientas de fecha y hora
│   ├── TonysMcpToolsVersion.cs     # Herramienta de versión
│   ├── NuGetToolsAdvanced.cs       # Herramientas NuGet (avanzadas)
│   ├── Program.cs                  # Punto de entrada y configuración del host
│   ├── Classes/                    # Modelos y clases de dominio
│   ├── Services/                   # Servicios internos
│   ├── Utiles/                     # Utilidades (DPAPI, helpers)
│   └── TonysMcpTools.csproj
├── TonysMcpTools.Tests/            # Proyecto de pruebas unitarias
└── TonysMcpToolsSolution.sln
```

---

## 📦 Dependencias principales

| Paquete | Versión |
|---|---|
| `ModelContextProtocol` | 0.3.0-preview.4 |
| `Microsoft.Extensions.Hosting` | 9.0.8 |
| `Serilog` + Sinks (File, Console) | 4.x / 6.x / 7.x / 9.x |
| `System.Security.Cryptography.ProtectedData` | 9.0.10 |
| `Google.Apis.Auth` | 1.72.0 |
| `CommunityToolkit.Mvvm` | 8.4.0 |

---

## 📝 Logging

Los logs se generan automáticamente en el directorio `logs/` dentro de la carpeta del ejecutable, con rotación diaria y retención de los últimos **30 días**.

---

## 📄 Licencia

Distribuido bajo la licencia **MIT**. Ver [LICENSE](LICENSE) para más información.

---

*© 2025 LicTony*
