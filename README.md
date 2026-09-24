# CarSales.Api

API REST para el ejercicio técnico de gestión y consulta de ventas de automóviles. El desarrollo se realizará de forma incremental, mediante commits pequeños y claros.

## Estado actual

Esta primera etapa contiene exclusivamente la solución y su arquitectura inicial. Aún no incluye dominio, ventas, endpoints, repositorios, persistencia ni pruebas funcionales.

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
└── .gitignore
```

## Responsabilidades

- `CarSales.Api`: punto de entrada de la API y composición de la aplicación.
- `CarSales.Application`: casos de uso y abstracciones de la aplicación.
- `CarSales.Domain`: reglas y modelos de negocio, sin dependencias externas.
- `CarSales.Infrastructure`: implementaciones técnicas de las abstracciones de Application.
- `CarSales.Tests`: pruebas unitarias principalmente de Domain y Application.

## Dependencias

```text
CarSales.Api → CarSales.Application → CarSales.Domain
CarSales.Infrastructure → CarSales.Application → CarSales.Domain
CarSales.Tests → CarSales.Application y CarSales.Domain
```

La solución utiliza .NET 10, nullable reference types e implicit usings habilitados. La funcionalidad se incorporará en etapas posteriores.
