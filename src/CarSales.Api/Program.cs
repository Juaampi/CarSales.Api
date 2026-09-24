using CarSales.Api.Configuration;
using CarSales.Application.Interfaces;
using CarSales.Application.Services;
using CarSales.Api.Middleware;
using CarSales.Application.Validators;
using CarSales.Infrastructure.Repositories;
using Asp.Versioning;
using Asp.Versioning.ApiExplorer;
using FluentValidation;
using FluentValidation.AspNetCore;
using Microsoft.OpenApi;
using Microsoft.Extensions.Options;
using Swashbuckle.AspNetCore.SwaggerGen;

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
	.AddApiVersioning(options =>
	{
		options.DefaultApiVersion = new ApiVersion(1, 0);
		options.ApiVersionReader = new UrlSegmentApiVersionReader();
		options.ReportApiVersions = true;
	})
	.AddApiExplorer(options =>
	{
		options.GroupNameFormat = "'v'VVV";
		options.SubstituteApiVersionInUrl = true;
	});
builder.Services
	.AddControllers()
	.AddJsonOptions(options =>
		options.JsonSerializerOptions.Converters.Add(new System.Text.Json.Serialization.JsonStringEnumConverter()));
builder.Services.AddFluentValidationAutoValidation();
builder.Services.AddValidatorsFromAssemblyContaining<CreateSaleRequestValidator>();
builder.Services.AddTransient<IConfigureOptions<SwaggerGenOptions>, ConfigureSwaggerOptions>();

// El Singleton conserva las ventas en memoria mientras la aplicación está levantada.
builder.Services.AddSingleton<ISaleRepository, SaleRepository>();
builder.Services.AddScoped<ISaleService, SaleService>();

var app = builder.Build();

app.UseSwagger();
var apiVersionDescriptionProvider = app.Services.GetRequiredService<IApiVersionDescriptionProvider>();
app.UseSwaggerUI(options =>
{
	foreach (var description in apiVersionDescriptionProvider.ApiVersionDescriptions)
	{
		options.SwaggerEndpoint(
			$"/swagger/{description.GroupName}/swagger.json",
			description.GroupName.ToUpperInvariant());
	}
});

// El tiempo mide todo el request; la API Key corta accesos inválidos antes del controller.
app.UseMiddleware<ExecutionTimeMiddleware>();
app.UseMiddleware<ApiKeyMiddleware>();
// Las excepciones inesperadas que atraviesan el pipeline se convierten en HTTP 500.
app.UseMiddleware<ExceptionHandlingMiddleware>();
app.UseAuthorization();

app.MapControllers();

app.Run();
