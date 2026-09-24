using Asp.Versioning.ApiExplorer;
using Microsoft.Extensions.Options;
using Microsoft.OpenApi;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace CarSales.Api.Configuration;

public sealed class ConfigureSwaggerOptions(
    IApiVersionDescriptionProvider apiVersionDescriptionProvider)
    : IConfigureOptions<SwaggerGenOptions>
{
    public void Configure(SwaggerGenOptions options)
    {
        foreach (var description in apiVersionDescriptionProvider.ApiVersionDescriptions)
        {
            options.SwaggerDoc(description.GroupName, new OpenApiInfo
            {
                Title = "Car Sales API",
                Version = description.GroupName,
                Description = description.IsDeprecated
                    ? "Esta versión de la API está obsoleta."
                    : "API para registrar y consultar ventas de vehículos."
            });
        }
    }
}