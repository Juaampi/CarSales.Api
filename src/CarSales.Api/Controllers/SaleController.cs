using CarSales.Application.DTOs;
using CarSales.Application.Interfaces;
using CarSales.Api.DTOs;
using Asp.Versioning;
using Microsoft.AspNetCore.Mvc;

namespace CarSales.Api.Controllers;

[ApiController]
[ApiVersion(1.0)]
[Route("api/v{version:apiVersion}/[controller]")]
public class SaleController(ISaleService saleService) : ControllerBase
{
    /// <summary>Obtiene el volumen total de unidades e importe de ventas.</summary>
    [HttpGet("total")]
    [ProducesResponseType(typeof(SalesTotalResponse), StatusCodes.Status200OK)]
    public async Task<ActionResult<SalesTotalResponse>> GetTotalSalesAsync()
    {
        var total = await saleService.GetTotalSalesAsync();

        return Ok(total);
    }

    /// <summary>Obtiene el volumen de ventas agrupado por centro de distribución.</summary>
    [HttpGet("by-center")]
    [ProducesResponseType(typeof(IReadOnlyCollection<SalesByCenterResponse>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyCollection<SalesByCenterResponse>>> GetSalesByCenterAsync()
    {
        var salesByCenter = await saleService.GetSalesByCenterAsync();

        return Ok(salesByCenter);
    }

    /// <summary>Obtiene el porcentaje de unidades de cada modelo por centro sobre el total general.</summary>
    [HttpGet("percentage-by-model")]
    [ProducesResponseType(
        typeof(IReadOnlyCollection<SalesPercentageByModelResponse>),
        StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyCollection<SalesPercentageByModelResponse>>>
        GetSalesPercentageByModelAsync()
    {
        var percentages = await saleService.GetSalesPercentageByModelAsync();

        return Ok(percentages);
    }

    /// <summary>Crea una venta calculando el precio unitario y el importe total.</summary>
    [HttpPost]
    [ProducesResponseType(typeof(SaleResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<SaleResponse>> CreateSaleAsync(CreateSaleRequest request)
    {
        var result = await saleService.CreateSaleAsync(request);

        if (result.IsFailure)
        {
            return BadRequest(new ErrorResponse(result.Error.Code, result.Error.Message));
        }

        return StatusCode(StatusCodes.Status201Created, SaleResponse.FromSale(result.Value));
    }
}
