# Decisiones técnicas

Este documento separa las decisiones de diseño y los supuestos del README operativo.

## Arquitectura

La solución utiliza una separación por capas inspirada en Clean Architecture:

```text
CarSales.Api
     |
     v
CarSales.Application
     |
     v
CarSales.Domain

CarSales.Infrastructure
     |
     v
CarSales.Application
     |
     v
CarSales.Domain

CarSales.Tests
     |
     +----> Application
     |
     +----> Domain
     |
     +----> Api
```

- **CarSales.Domain**: entidades y enums del negocio. No depende de infraestructura.
- **CarSales.Application**: servicios, DTOs, interfaces, reglas de negocio, `Result<T, Error>` y `Errors`.
- **CarSales.Infrastructure**: implementación del repositorio y almacenamiento en memoria.
- **CarSales.Api**: controllers, middleware, configuración, Swagger y API Key.
- **CarSales.Tests**: pruebas unitarias de Application, API, middleware e Infrastructure.

Application depende de abstracciones como `ISaleRepository`. Infrastructure implementa esas abstracciones mediante `SaleRepository`.

## Decisiones técnicas

### Separación por capas

La separación evita que la lógica de negocio dependa de HTTP, controllers o detalles de almacenamiento. API se ocupa de transporte y composición; Application coordina casos de uso; Domain contiene modelos; Infrastructure contiene implementaciones técnicas.

### Repository Pattern

Se utiliza `ISaleRepository` para desacoplar `SaleService` del mecanismo de almacenamiento. Esto permite reemplazar la implementación y mockear el repositorio en las pruebas.

### Repositorio en memoria

No se incorpora una base de datos porque el ejercicio solicita una solución simple y datos mockeables. `SaleRepository` conserva las ventas en memoria durante la ejecución de la aplicación.

### CSharpFunctionalExtensions

`Result<Sale, Error>` representa errores de negocio esperados sin utilizar excepciones como flujo normal. Actualmente `Errors.InvalidQuantity` representa una cantidad menor o igual a cero.

### Excepciones

Las excepciones quedan reservadas para errores inesperados. `ExceptionHandlingMiddleware` evita exponer detalles internos y devuelve `500 Internal Server Error` con un mensaje genérico.

### Middleware

La API utiliza middleware para responsabilidades transversales:

- `ExceptionHandlingMiddleware`: convierte excepciones inesperadas en respuestas HTTP 500.
- `ExecutionTimeMiddleware`: mide cada request con `Stopwatch` y registra su duración.
- `ApiKeyMiddleware`: protege los endpoints de negocio con el header `X-API-Key`.

El orden principal es:

```text
ExecutionTimeMiddleware
        |
        v
ApiKeyMiddleware
        |
        v
ExceptionHandlingMiddleware
        |
        v
Controllers
```

### ILogger

`ILogger` de ASP.NET Core registra la creación de ventas y la ejecución de las consultas principales. No se utiliza `Console.WriteLine` ni se registran API Keys.

### API Key

La API Key es un mecanismo simple de protección para este ejercicio, no un sistema completo de identidad o autorización. Se obtiene mediante Options Pattern desde configuración y puede ser sobrescrita mediante variables de entorno. Swagger permanece público para facilitar la prueba manual.

### Unit Tests

Las pruebas unitarias utilizan xUnit y Moq. Las dependencias se mockean mediante interfaces para aislar la lógica de Application y verificar llamadas al repositorio, resultados, errores y logging.

La cobertura medida sobre el código propio incluido en `coverage.runsettings` es aproximadamente 90% de líneas. El alcance excluye el bootstrap de `Program` y código generado por OpenAPI.

## Flujo de errores

```text
Error de negocio esperado
          |
          v
Result<T, Error>
          |
          v
Controller
          |
          v
HTTP 400
```

```text
Error inesperado
          |
          v
Exception
          |
          v
ExceptionHandlingMiddleware
          |
          v
HTTP 500
```

El controller convierte un `Result.Failure` en `400 Bad Request`. El middleware global no procesa `Result.Failure`; solo maneja excepciones inesperadas.

## Supuestos

1. Existen cuatro centros de distribución: `Center1`, `Center2`, `Center3` y `Center4`.
2. Existen cuatro modelos: `Sedan`, `SUV`, `Offroad` y `Sport`.
3. Sport tiene un precio base de USD 18.200 y aplica un impuesto del 7%, por lo que el precio unitario final es USD 19.474.
4. La cantidad debe ser mayor que cero. La aplicación devuelve `Sale.InvalidQuantity` cuando la cantidad es inválida.
5. El cliente no envía `Id`, `UnitPrice`, `TotalAmount` ni `CreatedAt`.
6. El porcentaje por modelo y centro se calcula sobre el total general de unidades vendidas.
7. Los centros y combinaciones de modelo sin ventas aparecen con unidades e importes/porcentajes en cero.
8. Los precios están definidos en `SaleService`; no son configurables desde la API.
9. Los datos se almacenan en memoria porque el ejercicio no requiere base de datos.
10. La API Key incluida en `appsettings.json` es ficticia y solo sirve para desarrollo local.

## Límites deliberados

El proyecto no incluye Entity Framework, base de datos, autenticación JWT/OAuth, MediatR, CQRS, estadísticas adicionales ni una capa de persistencia externa. Estas omisiones mantienen el alcance alineado con el ejercicio técnico.
