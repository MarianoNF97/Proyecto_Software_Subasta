# 🔨 SubastaYa — Plataforma de Subastas en Tiempo Real

[![.NET 8](https://img.shields.io/badge/.NET-8.0-512BD4?style=flat&logo=dotnet)](https://dotnet.microsoft.com/)
[![React 19](https://img.shields.io/badge/React-19-61DAFB?style=flat&logo=react)](https://react.dev/)
[![Tailwind CSS](https://img.shields.io/badge/Tailwind-v4-06B6D4?style=flat&logo=tailwindcss)](https://tailwindcss.com/)
[![SQL Server](https://img.shields.io/badge/SQL%20Server-2022-CC2927?style=flat&logo=microsoftsqlserver)](https://www.microsoft.com/sql-server)

## 📋 Descripción del Proyecto

**SubastaYa** es una plataforma web de subastas en tiempo real que permite a los usuarios publicar productos, pujar en subastas activas y gestionar una billetera virtual con sistema de escrow (garantía). El sistema implementa mecanismos avanzados de **concurrencia optimista**, **anti-sniping**, **liquidación automática** y **auditoría inmutable** para garantizar la integridad transaccional y la equidad del proceso de subasta.

### Modelo de Negocio

1. **Vendedores** publican subastas con precio base, incremento mínimo y fechas de apertura/cierre.
2. **Compradores** cargan saldo en su billetera y pujan en subastas activas. El sistema retiene (escrow) el monto ofertado automáticamente.
3. Al ser superado, el saldo retenido se libera instantáneamente.
4. Un **Worker en segundo plano** liquida las subastas vencidas: transfiere fondos al vendedor y marca la subasta como `FINALIZADA`, o la declara `DESIERTA` si no hubo ofertas.
5. El sistema protege contra **sniping** (pujas de último segundo) extendiendo la subasta 2 minutos automáticamente.

---

## 🏗️ Stack Tecnológico

| Capa | Tecnología |
|---|---|
| **Backend API** | .NET 8, ASP.NET Core Web API, Entity Framework Core 8 (Code-First) |
| **Base de Datos** | SQL Server (LocalDB / Express) |
| **Autenticación** | ASP.NET Core Identity + JWT Bearer Tokens |
| **Tiempo Real** | SignalR |
| **Frontend** | React 19 + Vite + Tailwind CSS v4 |
| **HTTP Client** | Axios (vía `apiClient.js`) |
| **Arquitectura Backend** | Clean Architecture + CQRS + Repository + Unit of Work |
| **Arquitectura Frontend** | SPA (Single Page Application) con React Router v6 |

---

## ⚙️ Instalación y Configuración

### Prerrequisitos

- [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0)
- [Node.js 18+](https://nodejs.org/) y [pnpm](https://pnpm.io/)
- [SQL Server](https://www.microsoft.com/sql-server) (LocalDB, Express o instancia completa)
- [EF Core CLI Tools](https://learn.microsoft.com/ef/core/cli/dotnet): `dotnet tool install --global dotnet-ef`

### 1. Clonar el Repositorio

```bash
git clone https://github.com/MarianoNF97/Proyecto_Software_Subasta.git
cd Proyecto_Software_Subasta
```

### 2. Configurar la Base de Datos

Editar el archivo `backend/SubastaYa.Api/appsettings.json` y ajustar la cadena de conexión:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=localhost;Database=SubastaYaDb;Trusted_Connection=True;TrustServerCertificate=True;"
  }
}
```

> **Nota**: Si usas SQL Server Express, cambiar `Server=localhost` por `Server=localhost\SQLEXPRESS`.

### 3. Ejecutar las Migraciones (Code-First)

```bash
cd backend
dotnet ef database update --project SubastaYa.Infrastructure --startup-project SubastaYa.Api
```

Esto crea la base de datos `SubastaYaDb` y ejecuta todas las migraciones + seeders automáticamente.

### 4. Iniciar el Backend

```bash
dotnet run --project SubastaYa.Api
```

El servidor estará disponible en `http://localhost:5094`. Swagger UI en `http://localhost:5094/swagger`.

### 5. Iniciar el Frontend

```bash
cd ../frontend
pnpm install
pnpm dev
```

El frontend estará disponible en `http://localhost:3000`. El proxy de Vite redirige `/api` al backend automáticamente.

---

## 🧪 Prueba de Concurrencia (Stress Test)

### ¿Cómo implementamos la concurrencia optimista?

Utilizamos el atributo `[Timestamp]` de EF Core en las entidades `Subasta` y `Billetera`, lo que genera una columna `rowversion` en SQL Server. Cuando dos transacciones intentan modificar el mismo registro simultáneamente, EF Core detecta la colisión y lanza `DbUpdateConcurrencyException`.

El middleware global (`ExceptionHandlingMiddleware`) intercepta esta excepción y retorna un HTTP **409 Conflict** con un mensaje descriptivo. Además, el handler `PlaceBidHandler` registra el intento fallido en la tabla de auditoría con la acción `PUJA_RECHAZADA_CONCURRENCIA`.

### ¿Cómo probarlo?

Se proveen scripts de prueba en el directorio `tests/` del proyecto. El test principal lanza **20 pujas simultáneas** desde distintos usuarios contra la misma subasta:

```bash
# Ejecutar el stress test masivo (requiere bash, curl y jq)
bash tests/stress_test_concurrencia.sh
```

**Resultado esperado**: Dado que los montos de puja son incrementales, el test evalúa el control de concurrencia optimista en un escenario real. Aquellas pujas que lleguen *exactamente* en el mismo milisegundo colisionarán, registrando la primera con `200/201` y rechazando las superpuestas con `409 Conflict`. Si por latencia llegan ligeramente desfasadas, el sistema permitirá las secuenciales o devolverá `400 Bad Request` si la puja es menor al nuevo precio. El script valida que la concurrencia proteja la integridad de la subasta exigiendo que las superposiciones sean correctamente bloqueadas.

---

## 📁 Estructura del Proyecto

```
Proyecto_Software_Subasta/
├── backend/
│   ├── SubastaYa.Api/              # Capa de Presentación (Controllers, Hubs, Workers, Middleware)
│   ├── SubastaYa.Application/      # Capa de Aplicación (CQRS Handlers, DTOs, Services, Interfaces)
│   ├── SubastaYa.Domain/           # Capa de Dominio (Entidades, Constantes)
│   ├── SubastaYa.Infrastructure/   # Capa de Infraestructura (EF Core, Repositories, Identity, Migrations)
│   └── SubastaYa.sln
├── frontend/
│   ├── src/
│   │   ├── components/             # Componentes React (AuctionCard, LiveBiddingRoom, etc.)
│   │   ├── contexts/               # AuthContext (manejo de sesión JWT)
│   │   ├── hooks/                  # Custom hooks (useCountdown)
│   │   ├── apiClient.js            # Cliente Axios con interceptores
│   │   └── App.jsx                 # Enrutamiento principal y catálogo
│   ├── vite.config.js
│   └── package.json
├── tests/                          # Scripts de testing (Stress Test, Anti-Sniping, Ledger)
└── README.md
```

---

## 👥 Equipo

Proyecto desarrollado para la cátedra de Ingeniería de Software.

