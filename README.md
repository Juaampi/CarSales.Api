# CarSales.Api

API REST para registrar y consultar ventas de una fábrica de automóviles. La aplicación calcula los precios en el backend, conserva las ventas en memoria y ofrece reportes por volumen, centro y modelo.

## Requisitos

- .NET SDK 10.0
- PowerShell, Bash o una terminal equivalente

## Ejecutar la aplicación

Desde la raíz del repositorio:

```bash
dotnet restore
dotnet build
dotnet run --project src/CarSales.Api/CarSales.Api.csproj
```

Con el perfil de desarrollo, la API queda disponible en `http://localhost:5152`.

Swagger UI:

```text
http://localhost:5152/swagger/index.html
```

La API Key no es necesaria para acceder a Swagger. En Swagger UI se puede utilizar `Authorize` para configurar el header `X-API-Key` y probar los endpoints de negocio.

## API Key

La key ficticia de desarrollo se configura en `src/CarSales.Api/appsettings.json`:

```json
{
	"ApiKey": {
		"Key": "development-api-key"
	}
}
```

Para sobreescribirla sin modificar archivos, utilizar la variable de entorno:

```text
ApiKey__Key=otra-key-de-desarrollo
```

Los endpoints `/api/Sale/*` requieren:

```http
X-API-Key: development-api-key
```

Una key ausente o inválida devuelve `401 Unauthorized`.

## Arquitectura resumida

```text
CarSales.Api
	|
	v
CarSales.Application
	|
	v
CarSales.Domain

CarSales.Infrastructure --> CarSales.Application --> CarSales.Domain
CarSales.Tests ---------> Application, Domain y Api
```

`CarSales.Domain` contiene entidades y enums. `CarSales.Application` contiene los casos de uso, DTOs, interfaces y reglas de negocio. `CarSales.Infrastructure` implementa el repositorio en memoria. `CarSales.Api` compone la aplicación y contiene controllers, middleware, configuración, Swagger y seguridad. Las decisiones detalladas están en [docs/decisiones-tecnicas.md](docs/decisiones-tecnicas.md).

## Endpoints

| Método | Endpoint | Descripción |
|---|---|---|
| `POST` | `/api/Sale` | Registra una venta y calcula sus importes. |
| `GET` | `/api/Sale/total` | Obtiene unidades e importe total de ventas. |
| `GET` | `/api/Sale/by-center` | Obtiene unidades e importe agrupados por centro. |
| `GET` | `/api/Sale/percentage-by-model` | Obtiene el porcentaje de unidades de cada modelo por centro. |

Todos los endpoints de ventas requieren `X-API-Key`.

## Crear una venta

Request:

```bash
curl -X POST "http://localhost:5152/api/Sale" \
	-H "Content-Type: application/json" \
	-H "X-API-Key: development-api-key" \
	-d '{
		"model": "Sport",
		"distributionCenter": "Center1",
		"quantity": 2
	}'
```

Response `201 Created`:

```json
{
	"id": "guid-generado",
	"model": "Sport",
	"distributionCenter": "Center1",
	"quantity": 2,
	"unitPrice": 19474,
	"totalAmount": 38948,
	"createdAt": "2026-09-24T19:00:00+00:00"
}
```

El cliente envía únicamente modelo, centro y cantidad. `Id`, `UnitPrice`, `TotalAmount` y `CreatedAt` son generados o calculados por la aplicación.

## Consultas

### Volumen total

```bash
curl "http://localhost:5152/api/Sale/total" \
	-H "X-API-Key: development-api-key"
```

```json
{
	"totalUnits": 10,
	"totalAmount": 108474
}
```

### Volumen por centro

```bash
curl "http://localhost:5152/api/Sale/by-center" \
	-H "X-API-Key: development-api-key"
```

```json
[
	{
		"distributionCenter": "Center1",
		"totalUnits": 5,
		"totalAmount": 77422
	},
	{
		"distributionCenter": "Center2",
		"totalUnits": 0,
		"totalAmount": 0
	},
	{
		"distributionCenter": "Center3",
		"totalUnits": 0,
		"totalAmount": 0
	},
	{
		"distributionCenter": "Center4",
		"totalUnits": 0,
		"totalAmount": 0
	}
]
```

La respuesta siempre incluye los cuatro centros definidos por el dominio.

### Porcentaje por modelo y centro

```bash
curl "http://localhost:5152/api/Sale/percentage-by-model" \
	-H "X-API-Key: development-api-key"
```

La respuesta contiene las 16 combinaciones posibles entre los cuatro centros y los cuatro modelos:

```json
[
	{
		"distributionCenter": "Center1",
		"model": "Sedan",
		"units": 2,
		"percentage": 20
	}
]
```

Las combinaciones sin ventas aparecen con `units: 0` y `percentage: 0`.

La fórmula utilizada es:

```text
unidades del modelo en el centro / total general de unidades vendidas * 100
```

Por ejemplo, `Center1 / Sedan = 2` unidades sobre un total general de `10` unidades produce `20%`. El porcentaje se calcula sobre el total general, no sobre el total del centro.

## Precios

| Modelo | Precio base |
|---|---:|
| Sedan | USD 8.000 |
| SUV | USD 9.500 |
| Offroad | USD 12.500 |
| Sport | USD 18.200 + 7% de impuesto |

El precio final de Sport se calcula en Application:

```text
18.200 * 1,07 = USD 19.474
```

Los importes utilizan `decimal` y el precio no es enviado por el cliente.

## Tests y cobertura

Ejecutar todos los tests:

```bash
dotnet test
```

La cobertura configurada para código propio se puede obtener con:

```bash
dotnet test --settings coverage.runsettings --collect:"XPlat Code Coverage"
```

La medición actual es aproximadamente **90% de líneas** sobre Application, Domain, Infrastructure, Controllers y Middleware. El reporte excluye el bootstrap de `Program` y código generado por OpenAPI.

## Estructura

```text
CarSales.Api/
├── src/
│   ├── CarSales.Api/
│   ├── CarSales.Application/
│   ├── CarSales.Domain/
│   └── CarSales.Infrastructure/
├── tests/
│   └── CarSales.Tests/
├── CarSales.sln
├── README.md
└── coverage.runsettings
```

Las decisiones de arquitectura, errores, seguridad, persistencia y supuestos del ejercicio están documentadas por separado en [docs/decisiones-tecnicas.md](docs/decisiones-tecnicas.md).
