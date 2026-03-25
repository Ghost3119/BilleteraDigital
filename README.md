# Billetera Digital — Backend API

**API REST para Billetera Digital** construida con **.NET 9** y **Arquitectura Hexagonal (Ports & Adapters)**. Proporciona endpoints seguros para gestión de cuentas, transferencias y autenticación JWT.

---

## 📋 Tabla de contenidos

- [Requisitos previos](#-requisitos-previos)
- [Arquitectura del proyecto](#-arquitectura-del-proyecto)
- [Instalación](#-instalación)
- [Configuración de la base de datos](#️-configuración-de-la-base-de-datos)
- [Migraciones de Entity Framework Core](#-migraciones-de-entity-framework-core)
- [Ejecución](#-ejecución)
- [Endpoints principales](#-endpoints-principales)
- [Autenticación y seguridad](#-autenticación-y-seguridad)
- [Troubleshooting](#-troubleshooting)

---

## 🔧 Requisitos previos

Antes de comenzar, asegúrate de tener instalado lo siguiente:

- **.NET SDK 9.0** o superior — [Descargar aquí](https://dotnet.microsoft.com/download/dotnet/9.0)
- **SQL Server** — Una de las siguientes opciones:
  - **SQL Server LocalDB** (recomendado para desarrollo en Windows)
  - **SQL Server Express**
  - **SQL Server en Docker** (Linux/macOS)
  - **Azure SQL Database** (producción)

### Verificar instalación de .NET

```bash
dotnet --version
# Debe mostrar: 9.0.xxx
```

### Verificar SQL Server LocalDB (Windows)

```bash
sqllocaldb info
# Debe mostrar: MSSQLLocalDB
```

Si LocalDB no está disponible, instálalo desde [SQL Server Express](https://www.microsoft.com/sql-server/sql-server-downloads).

---

## 📐 Arquitectura del proyecto

Este proyecto implementa **Arquitectura Hexagonal** (también conocida como Ports & Adapters), separando las responsabilidades en capas claramente definidas:

```
src/
├── BilleteraDigital.Domain/          # Capa de Dominio (Core)
│   ├── Entities/                     # Entidades de negocio (Cuenta, Usuario, Transaccion)
│   ├── Enums/                        # Tipos enumerados (TipoTransaccion, EstadoCuenta)
│   └── Exceptions/                   # Excepciones de dominio (SaldoInsuficienteException, etc.)
│
├── BilleteraDigital.Application/     # Capa de Aplicación (Use Cases)
│   ├── UseCases/                     # Casos de uso del negocio
│   │   ├── Auth/                     # Login, registro
│   │   ├── Cuenta/                   # Consultar cuentas, historial
│   │   ├── Transferencia/            # Realizar transferencias
│   │   └── Usuario/                  # Gestión de usuarios
│   ├── Ports/                        # Interfaces (contratos)
│   │   ├── Repositories/             # IUsuarioRepository, ICuentaRepository, etc.
│   │   └── Services/                 # IJwtService, IPasswordHasher, IUnitOfWork
│   ├── Common/                       # Clases compartidas (Result, PagedResult, GenericQueryParams)
│   └── Mappings/                     # Perfiles de AutoMapper
│
└── BilleteraDigital.API/             # Capa de Infraestructura + API (Adaptadores)
    ├── Controllers/                  # Controladores REST (AuthController, CuentasController, etc.)
    ├── Infrastructure/               # Implementaciones concretas
    │   ├── Persistence/              # EF Core, DbContext, Repositories
    │   └── Security/                 # JwtService, PasswordHasher
    ├── Middleware/                   # GlobalExceptionMiddleware
    ├── Extensions/                   # ServiceCollectionExtensions (DI setup)
    ├── Migrations/                   # Migraciones de EF Core
    ├── appsettings.json              # Configuración global
    ├── appsettings.Development.json  # Configuración de desarrollo
    └── Program.cs                    # Punto de entrada
```

### Principios clave

| Capa | Responsabilidad | Depende de |
|------|----------------|------------|
| **Domain** | Lógica de negocio pura, sin dependencias externas | Nada (núcleo independiente) |
| **Application** | Casos de uso, orquestación, DTOs | Solo Domain |
| **API (Infrastructure)** | Adaptadores, persistencia, seguridad, controladores | Application + Domain |

---

## 📦 Instalación

Clona el repositorio y navega al directorio del backend:

```bash
cd BilleteraDigital
```

Restaura las dependencias del proyecto:

```bash
dotnet restore
```

---

## ⚙️ Configuración de la base de datos

### Opción 1: SQL Server LocalDB (Windows — Recomendado)

La configuración por defecto usa LocalDB. No se requiere ningún cambio si ya tienes LocalDB instalado.

El archivo `appsettings.json` ya contiene la cadena de conexión predeterminada:

```json
{
  "ConnectionStrings": {
    "BilleteraDigital": "Server=(localdb)\\MSSQLLocalDB;Database=BilleteraDigitalDB;Trusted_Connection=True;TrustServerCertificate=True;"
  }
}
```

### Opción 2: SQL Server Express / Full

Si usas SQL Server Express o una instancia completa, actualiza la cadena de conexión en `appsettings.Development.json`:

```json
{
  "ConnectionStrings": {
    "BilleteraDigital": "Server=localhost;Database=BilleteraDigitalDB;User Id=sa;Password=TuPasswordSeguro;TrustServerCertificate=True;"
  }
}
```

### Opción 3: SQL Server en Docker

Inicia un contenedor de SQL Server:

```bash
docker run -e "ACCEPT_EULA=Y" -e "MSSQL_SA_PASSWORD=YourStrong!Passw0rd" -p 1433:1433 --name sqlserver -d mcr.microsoft.com/mssql/server:2022-latest
```

Luego actualiza `appsettings.Development.json`:

```json
{
  "ConnectionStrings": {
    "BilleteraDigital": "Server=localhost,1433;Database=BilleteraDigitalDB;User Id=sa;Password=YourStrong!Passw0rd;TrustServerCertificate=True;"
  }
}
```

---

## 🗄️ Migraciones de Entity Framework Core

El proyecto ya incluye las migraciones necesarias en `BilleteraDigital.API/Migrations/`. Solo necesitas aplicarlas a tu base de datos.

### Instalar EF Core CLI (si no lo tienes)

```bash
dotnet tool install --global dotnet-ef
```

Verifica la instalación:

```bash
dotnet ef --version
# Debe mostrar: Entity Framework Core .NET Command-line Tools 9.x.x
```

### Aplicar migraciones y crear la base de datos

Desde la **raíz del repositorio** (donde está el archivo `.sln`):

```bash
dotnet ef database update --project src/BilleteraDigital.API --startup-project src/BilleteraDigital.API
```

Este comando:
1. Lee la cadena de conexión de `appsettings.json`
2. Crea la base de datos `BilleteraDigitalDB` si no existe
3. Ejecuta todas las migraciones pendientes
4. Crea las tablas: `Usuarios`, `Cuentas`, `Transacciones`

### Verificar que la base de datos se creó correctamente

**En LocalDB:**

```bash
sqllocaldb info MSSQLLocalDB
```

Luego, conéctate usando **SQL Server Management Studio (SSMS)** o **Azure Data Studio** con:
- Server: `(localdb)\MSSQLLocalDB`
- Authentication: Windows Authentication

Deberías ver la base de datos `BilleteraDigitalDB` con 3 tablas.

---

## 🚀 Ejecución

### Opción 1: Ejecutar con `dotnet run`

Desde la raíz del proyecto:

```bash
cd src/BilleteraDigital.API
dotnet run
```

O directamente desde la raíz con el parámetro `--project`:

```bash
dotnet run --project src/BilleteraDigital.API
```

La API se iniciará en:

```
http://localhost:5112
https://localhost:7268
```

### Opción 2: Ejecutar con `dotnet watch` (Hot Reload)

Para desarrollo con recarga automática:

```bash
cd src/BilleteraDigital.API
dotnet watch run
```

### Acceder a Swagger UI

Una vez que la API esté corriendo, abre tu navegador y ve a:

```
http://localhost:5112/swagger
```

Swagger UI te permite:
- Ver todos los endpoints disponibles
- Probar requests directamente desde el navegador
- Ver los esquemas de DTOs y respuestas

---

## 🌐 Endpoints principales

| Endpoint | Método | Descripción | Auth |
|----------|--------|-------------|------|
| `/api/v1/Auth/token` | POST | Iniciar sesión (obtener JWT) | ❌ Público |
| `/api/v1/Usuarios/registrar` | POST | Registrar nuevo usuario | ❌ Público |
| `/api/v1/Auth/refresh` | POST | Renovar token JWT | ❌ Público |
| `/api/v1/Cuentas/crear` | POST | Crear nueva cuenta | ✅ Requiere JWT |
| `/api/v1/Cuentas/mias` | GET | Listar cuentas del usuario autenticado | ✅ Requiere JWT |
| `/api/v1/Cuentas/{id}/saldo` | GET | Consultar saldo de una cuenta | ✅ Requiere JWT |
| `/api/v1/Cuentas/{id}/transacciones` | GET | Historial de transacciones (paginado) | ✅ Requiere JWT |
| `/api/v1/Transferencias/realizar` | POST | Realizar transferencia entre cuentas | ✅ Requiere JWT |

### Ejemplo de request: Registrar usuario

```bash
curl -X POST http://localhost:5112/api/v1/Usuarios/registrar \
  -H "Content-Type: application/json" \
  -d '{
    "nombre": "Juan Pérez",
    "email": "juan@example.com",
    "password": "Password123!"
  }'
```

### Ejemplo de request: Login

```bash
curl -X POST http://localhost:5112/api/v1/Auth/token \
  -H "Content-Type: application/json" \
  -d '{
    "email": "juan@example.com",
    "password": "Password123!"
  }'
```

Respuesta:

```json
{
  "accessToken": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...",
  "refreshToken": "base64string...",
  "expiraEn": "60 minutos",
  "tipo": "Bearer"
}
```

### Ejemplo de request autenticado: Consultar cuentas

```bash
curl -X GET http://localhost:5112/api/v1/Cuentas/mias \
  -H "Authorization: Bearer TU_ACCESS_TOKEN_AQUI"
```

---

## 🔐 Autenticación y seguridad

### JWT (JSON Web Tokens)

La API utiliza **autenticación basada en JWT** con los siguientes claims:

- `sub` → ID del usuario (GUID)
- `unique_name` → Nombre del usuario
- `email` → Correo electrónico
- `jti` → ID único del token
- `iat` → Timestamp de emisión

### Configuración JWT

La configuración de JWT se encuentra en `appsettings.json`:

```json
{
  "Jwt": {
    "SecretKey": "BilleteraDigital-SuperSecretKey-AtLeast32Chars!!",
    "Issuer": "BilleteraDigital",
    "Audience": "BilleteraDigital",
    "ExpiresMinutes": "60"
  }
}
```

> **⚠️ IMPORTANTE:** En producción, la `SecretKey` debe ser una cadena aleatoria segura de al menos 32 caracteres y almacenarse en variables de entorno o Azure Key Vault, **NUNCA en código fuente**.

### Refresh Tokens

La API soporta **rotación de tokens**:

1. Al iniciar sesión, el usuario recibe un `accessToken` (vida corta: 60 min) y un `refreshToken` (vida larga: 7 días)
2. Cuando el `accessToken` expira, el cliente puede usar `/api/v1/Auth/refresh` para obtener un nuevo par de tokens sin requerir credenciales
3. Cada rotación invalida el `refreshToken` anterior

---

## 🐛 Troubleshooting

### Error: "Cannot open database 'BilleteraDigitalDB'"

**Solución:** Ejecuta las migraciones:

```bash
dotnet ef database update --project src/BilleteraDigital.API
```

### Error: "Cannot connect to SQL Server LocalDB"

**Solución:** Verifica que LocalDB esté corriendo:

```bash
sqllocaldb info MSSQLLocalDB
sqllocaldb start MSSQLLocalDB
```

Si el proceso `sqlservr.exe` está en estado zombie:

```powershell
Stop-Process -Name sqlservr -Force
sqllocaldb start MSSQLLocalDB
```

### Error: "Failed to load configuration"

**Solución:** Verifica que `appsettings.json` esté correctamente formateado (JSON válido) y que la cadena de conexión sea correcta.

### Error: "The ConnectionString property has not been initialized"

**Solución:** Asegúrate de que `appsettings.json` contenga la sección `ConnectionStrings` con la clave `BilleteraDigital`.

### Port 5112 ya está en uso

**Solución:** Cambia el puerto en `src/BilleteraDigital.API/Properties/launchSettings.json` o detén el proceso que está usando el puerto.

---

## 📜 Scripts útiles

```bash
# Compilar el proyecto
dotnet build --configuration Release

# Ejecutar tests (si existen)
dotnet test

# Limpiar artefactos de compilación
dotnet clean

# Crear una nueva migración
dotnet ef migrations add <NombreMigracion> --project src/BilleteraDigital.API

# Revertir la última migración
dotnet ef database update <MigracionAnterior> --project src/BilleteraDigital.API

# Eliminar la última migración (si no se ha aplicado)
dotnet ef migrations remove --project src/BilleteraDigital.API

# Ver el SQL que generaría una migración
dotnet ef migrations script --project src/BilleteraDigital.API
```

---

## 📝 Convenciones de código

### Nombres de clases y archivos

- **Entidades de dominio** → `PascalCase.cs` (ej: `Usuario.cs`, `Cuenta.cs`)
- **DTOs** → Sufijo `Dto`, `Request` o `Response` (ej: `CuentaDto.cs`, `LoginRequest.cs`)
- **Casos de uso** → Verbo + Sustantivo (ej: `ConsultarMisCuentas.cs`, `RealizarTransferencia.cs`)
- **Repositorios** → Prefijo `I` para interfaces (ej: `IUsuarioRepository.cs`)

### Idioma

- **Código** → Español (clases, propiedades, métodos)
- **Commits** → Español
- **Documentación** → Español

### Paginación

- Todos los endpoints que devuelven colecciones **DEBEN** usar paginación
- Patrón: `GenericQueryParams` + `PagedResult<T>` + header `X-Pagination`

---

## 📞 Soporte

Si tienes problemas o preguntas, contacta al equipo de desarrollo.

---

**¡La API está lista para usarse! 🚀**
