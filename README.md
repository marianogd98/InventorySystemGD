# 📦 Mini Sistema de Gestión de Inventario (InventorySystemGD)

Proyecto desarrollado para la evaluación técnica de desarrollador .NET Semi-Senior. Consiste en una solución empresarial basada en **Clean Architecture**, **CQRS**, **ASP.NET Core 8 Web API**, **ASP.NET Core Razor Pages**, **Entity Framework Core**, **Dapper**, **SQL Server** y **Docker**.

---

## 🏛️ Arquitectura del Sistema

La solución sigue los principios de **Clean Architecture** (Arquitectura Limpia) y el patrón **CQRS** (Command Query Responsibility Segregation) para separar responsabilidades de escritura y lectura:

```
InventorySystemGD/
├── Inventory.Domain/          # Entidades, Agregados (DDD), Invariantes de Negocio e Interfaces
├── Inventory.Application/     # CQRS con MediatR (Commands, Queries, DTOs y Handlers)
├── Inventory.Infrastructure/  # EF Core (Escritura), Dapper (Lectura) y Persistencia
├── Inventory.Api/             # ASP.NET Core Web API REST protegida con JWT y Serilog
├── Inventory.Web/             # ASP.NET Core Razor Pages con Bootstrap 5.3 y SweetAlert2
├── Inventory.UnitTests/       # Pruebas unitarias con xUnit y Moq
└── docker-compose.yml         # Contenedor oficial de SQL Server 2022
```

### ⚡ Patrón CQRS & Estrategia de Persistencia Híbrida:
* **Escrituras (Commands):** Procesadas mediante **Entity Framework Core** (`ApplicationDbContext`), asegurando la encapsulación de reglas de dominio mediante métodos de fábrica (*Factory Methods* de DDD) y control transaccional atómico (`SaveChangesAsync`).
* **Lecturas (Queries):** Procesadas mediante el micro-ORM **Dapper** (`InventoryQueries`), garantizando máximo rendimiento, sin sobrecarga de seguimiento de estados (*No Tracking*), ejecutando consultas SQL directas y consumiendo un **Procedimiento Almacenado (T-SQL)**.

---

## 📋 Requerimientos y Cumplimiento

| Requerimiento Solicitado | Estado | Implementación / Ubicación |
| :--- | :---: | :--- |
| **Clean Architecture & CQRS con MediatR** | ✅ | Desacoplado en 5 capas con `MediatR` para commands y queries. |
| **Endpoint `POST /api/products` (JWT)** | ✅ | Creación de productos con validación DDD y persistencia EF Core. |
| **Endpoint `GET /api/products/low-stock` (JWT)** | ✅ | Consulta de productos con stock crítico (≤ 10) usando Dapper. |
| **Autenticación JWT** | ✅ | `POST /api/auth/login` con Claims y firma HMAC SHA-256 (`[Authorize]`). |
| **Logging estructurado con Serilog** | ✅ | Configurado en `Inventory.Api` con consola enriquecida y middleware HTTP. |
| **Pruebas Unitarias** | ✅ | 11 pruebas automatizadas con `xUnit` y `Moq` (casos de éxito, error y seguridad). |
| **EF Core (Escrituras) + Dapper (Lecturas)** | ✅ | `ProductRepository` (EF Core) + `InventoryQueries` (Dapper). |
| **Procedimiento Almacenado T-SQL** | ✅ | `sp_GetInventoryValueByCategory` invocado desde Dapper. |
| **Frontend Razor Pages + Bootstrap** | ✅ | `Inventory.Web` con métricas KPI, tablas y modal interactivo. |
| **SweetAlert2 Integrado** | ✅ | Modales modernos para alertas de éxito, advertencias y errores. |
| **Modo Oscuro / Modo Claro** | ✅ | Alternador dinámico en navbar con persistencia en `localStorage`. |
| **Validación Anti Inyección SQL** | ✅ | Expresiones regulares y filtros en Dominio, Backend e Interfaz. |

---

## 🚀 Guía de Instalación y Ejecución

### 1. Prerrequisitos
* [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0) instalado.
* [Docker Desktop](https://www.docker.com/products/docker-desktop/) (o una instancia local de SQL Server 2019/2022).

---

### 2. Base de Datos (SQL Server con Docker)

1. Inicia el contenedor de SQL Server ejecutando en la raíz del proyecto:
   ```powershell
   docker compose up -d
   ```
2. La base de datos `InventoryDb`, las tablas, índices optimizados y el **Procedimiento Almacenado** se inicializan automáticamente. Si deseas ejecutar el script manualmente en SQL Server Management Studio (SSMS) o Azure Data Studio, utiliza el archivo ubicado en:
   * [`scripts/01_CreateProductsAndStoredProcedure.sql`](scripts/01_CreateProductsAndStoredProcedure.sql)

**Credenciales por defecto de la BD:**
* **Host:** `localhost,1433`
* **Usuario:** `sa`
* **Contraseña:** `SuperSecret123!`
* **Base de Datos:** `InventoryDb`

---

### 3. Ejecutar la Web API (`Inventory.Api`)

Abre una terminal en la raíz y ejecuta:
```powershell
dotnet run --project Inventory.Api
```
* **Swagger UI:** [http://localhost:5102/swagger](http://localhost:5102/swagger)
* **Credenciales JWT de prueba:**
  * **Usuario:** `admin`
  * **Contraseña:** `admin123`

---

### 4. Ejecutar el Cliente Web (`Inventory.Web`)

Abre una segunda terminal y ejecuta:
```powershell
dotnet run --project Inventory.Web
```
*(O con recarga automática: `dotnet watch --project Inventory.Web`)*

* **Aplicación Web:** [http://localhost:5032](http://localhost:5032)

---

### 5. Ejecutar las Pruebas Unitarias

Para correr las pruebas automatizadas del comando de creación y validaciones de negocio:
```powershell
dotnet test
```

---

## 🔒 Endpoints de la API REST

| Método | Ruta | Autenticación | Descripción |
| :--- | :--- | :---: | :--- |
| `POST` | `/api/auth/login` | Anónima | Genera un token JWT para autenticar peticiones. |
| `POST` | `/api/products` | `Bearer Token` | Registra un nuevo producto mediante Command (EF Core). |
| `GET` | `/api/products/low-stock?threshold=10` | `Bearer Token` | Lista productos con stock crítico (Dapper). |
| `GET` | `/api/products/inventory-value-by-category` | `Bearer Token` | Obtiene el resumen valorizado mediante Stored Procedure (Dapper). |

---

## 🎨 Características Adicionales del Frontend
* **Modo Oscuro / Claro:** Detección de preferencia del sistema operativo y selector persistente en la barra de navegación.
* **Notificaciones SweetAlert2:** Retroalimentación visual interactiva en la creación de productos y errores de validación.
* **Seguridad:** Sanitización de entradas contra inyecciones SQL en cliente y servidor.

---

## 🔄 Git Flow & Integración Continua (CI/CD con GitHub Actions)

El proyecto cuenta con integración y despliegue continuo automatizado configurado en `.github/workflows/`:

```
              ┌───────────────┐
              │  mariano-dev  │ (Desarrollo de nuevas funcionalidades)
              └───────┬───────┘
                      │ PR (CI Pipeline: Build + Tests)
                      ▼
              ┌───────────────┐
              │      dev      │ (Integración continua / QA)
              └───────┬───────┘
                      │ Merge / Tag Release
                      ▼
              ┌───────────────┐
              │     prod      │ (CD Pipeline: Publicación de Artefactos de Release)
              └───────────────┘
```

1. **Pipeline de Integración Continua ([`ci.yml`](.github/workflows/ci.yml)):**
   * **Disparadores:** `push` y `pull_request` sobre ramas `prod`, `main`, `dev`, `mariano-dev`, `feature/**`, `bugfix/**`, `hotfix/**`.
   * **Acciones:**
     * Restauración con caché de paquetes NuGet.
     * Compilación en modo Release.
     * Ejecución de suite de pruebas unitarias (`xUnit`).
     * Publicación de reporte de cobertura y resultados de pruebas como artefactos.

2. **Pipeline de Despliegue / Entrega Continua ([`cd.yml`](.github/workflows/cd.yml)):**
   * **Disparadores:** `push` a ramas productivas (`prod`, `main`), etiquetas de versión (`v*.*.*`) o ejecución manual (`workflow_dispatch`).
   * **Acciones:**
     * Publicación optimizada de binarios de `Inventory.Api` y `Inventory.Web`.
     * Generación y empaquetado de artefactos descargables listos para despliegue.