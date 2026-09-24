using CarSales.Application.DTOs;
using CarSales.Application.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace CarSales.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class SaleController(ISaleService saleService) : ControllerBase
{
    [HttpGet("total")]
    [ProducesResponseType(typeof(SalesTotalResponse), StatusCodes.Status200OK)]
    public async Task<ActionResult<SalesTotalResponse>> GetTotalSalesAsync()
    {
        var total = await saleService.GetTotalSalesAsync();

        return Ok(total);
    }

    [HttpPost]
    [ProducesResponseType(typeof(SaleResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<SaleResponse>> CreateSaleAsync(CreateSaleRequest request)
    {
        try
        {
            var sale = await saleService.CreateSaleAsync(request);

            return StatusCode(StatusCodes.Status201Created, SaleResponse.FromSale(sale));
        }
        catch (ArgumentException exception)
        {
            return BadRequest(new ProblemDetails
            {
                Title = "Invalid sale",
                Detail = exception.Message,
                Status = StatusCodes.Status400BadRequest
            });
        }
    }
}
