## Ejecutar el script que esta en la carpeta BD Script

> **Importante:** Para ejecutar los endpoints , **ejecuta el script**   

## Base de datos (SQL Server)

La API usa **SQL Server** con **EF Core 7**.

### Cadena de conexión (appsettings.*.json)
```jsonc
{
  "ConnectionStrings": {
    "Default": "Server=localhost,1433;Database=RealEstateDb;User Id=sa;Password=<TuPass123!>;TrustServerCertificate=true;MultipleActiveResultSets=true"
    // alternativa localdb:
    // "Default": "Server=(localdb)\\MSSQLLocalDB;Database=RealEstateDb;Trusted_Connection=True;MultipleActiveResultSets=true"
  }
}


## Estructura de la solución

- `RealEstate.Domain` – Entidades del dominio.
- `RealEstate.Application` – Casos de uso (MediatR), DTOs, validadores y perfiles de AutoMapper.
- `RealEstate.Infrastructure` – Servicios .
- `RealState-Million` – API ASP.NET Core (.NET 7) + Swagger + JWT.
- `RealEstate.Tests` – Pruebas unitarias.


## Configuración

`appsettings.json` 


## Antes de probar los endpoints (Auth obligatorio)

> **Requisito:** Todos los endpoints protegidos requieren **JWT**. 

### 1) Generar el token
Endpoint: `POST /api/Auth/token`  
Body:
```json
{ "user": "wilmar", "password": "wilmar" }