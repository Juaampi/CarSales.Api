using CarSales.Api.Configuration;
using CarSales.Application.Interfaces;
using CarSales.Application.Services;
using CarSales.Api.Middleware;
using CarSales.Infrastructure.Repositories;
using Microsoft.OpenApi;

var builder = WebApplication.CreateBuilder(args);

// Swagger queda público para poder explorar y probar la API desde el navegador.

builder.Services.AddSwaggerGen(options =>
{
	options.IncludeXmlComments(Path.Combine(AppContext.BaseDirectory, "CarSales.Api.xml"));

	options.AddSecurityDefinition("ApiKey", new OpenApiSecurityScheme
	{
		Description = "API Key sent in the X-API-Key header.",
		Name = "X-API-Key",
		In = ParameterLocation.Header,
		Type = SecuritySchemeType.ApiKey
	});

	options.AddSecurityRequirement(document => new OpenApiSecurityRequirement
	{
		[new OpenApiSecuritySchemeReference("ApiKey", document, null)] = []
	});
});
builder.Services.Configure<ApiKeyOptions>(builder.Configuration.GetSection("ApiKey"));
builder.Services
	.AddControllers()
	.AddJsonOptions(options =>
		options.JsonSerializerOptions.Converters.Add(new System.Text.Json.Serialization.JsonStringEnumConverter()));

// El Singleton conserva las ventas en memoria mientras la aplicación está levantada.
builder.Services.AddSingleton<ISaleRepository, SaleRepository>();
builder.Services.AddScoped<ISaleService, SaleService>();

var app = builder.Build();

app.UseSwagger();
app.UseSwaggerUI();

// El tiempo mide todo el request; la API Key corta accesos inválidos antes del controller.
app.UseMiddleware<ExecutionTimeMiddleware>();
app.UseMiddleware<ApiKeyMiddleware>();
// Las excepciones inesperadas que atraviesan el pipeline se convierten en HTTP 500.
app.UseMiddleware<ExceptionHandlingMiddleware>();
app.UseAuthorization();

app.MapControllers();

app.Run();
