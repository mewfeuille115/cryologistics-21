# Migraciones EF Core

Comandos de referencia para gestionar migraciones de Entity Framework Core en el servicio de usuarios.

## Agregar migración

```powershell
Add-Migration <nombre_migracion> `
  -Context UsersDbContext `
  -Project <proyecto_de_infraestructura> `
  -StartupProject <nombre_del_servicio> `
  -o Database/Migrations
```

**Ejemplo:**

```powershell
Add-Migration create_database `
  -Context UsersDbContext `
  -Project Cryologistics21.Services.Users.Infrastructure `
  -StartupProject Cryologistics21.Services.Users.Api `
  -o Database/Migrations
```

| Parámetro         | Descripción                                                                   |
| ----------------- | ----------------------------------------------------------------------------- |
| `<nombre>`        | Nombre descriptivo de la migración (ej. `create_database`, `add_users_table`) |
| `-Context`        | DbContext objetivo                                                            |
| `-Project`        | Proyecto que contiene el DbContext y las migraciones                          |
| `-StartupProject` | Proyecto de inicio que provee la configuración (cadena de conexión, etc.)     |
| `-o`              | Carpeta de salida relativa al proyecto de infraestructura                     |

---

## Eliminar última migración

```powershell
Remove-Migration -Force `
  -Context UsersDbContext `
  -Project Cryologistics21.Services.Users.Infrastructure `
  -StartupProject Cryologistics21.Services.Users.Api
```

> **`-Force`** revierte la migración si ya fue aplicada a la base de datos antes de eliminarla.

---

## Notas

- Ejecutar los comandos desde la **Package Manager Console** de Visual Studio, o con `dotnet ef` desde la raíz de la solución.
- Siempre revisar el archivo de migración generado antes de aplicarlo en entornos no locales.
