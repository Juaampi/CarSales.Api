using CarSales.Api.Configuration;
using CarSales.Application.Interfaces;
using CarSales.Application.Services;
using CarSales.Api.Middleware;
using CarSales.Infrastructure.Repositories;
using Microsoft.OpenApi;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

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

// Singleton keeps in-memory sales for the lifetime of the running application.
builder.Services.AddSingleton<ISaleRepository, SaleRepository>();
builder.Services.AddScoped<ISaleService, SaleService>();

var app = builder.Build();

// Configure the HTTP request pipeline.

app.UseSwagger();
app.UseSwaggerUI();

app.UseMiddleware<ExecutionTimeMiddleware>();
app.UseMiddleware<ApiKeyMiddleware>();
app.UseMiddleware<ExceptionHandlingMiddleware>();
app.UseAuthorization();

app.MapControllers();

app.Run();
