using Microsoft.AspNetCore.Mvc;
using Tutorial9.Model.DTOs;
using Tutorial9.Services;

namespace Tutorial9.Controllers;

[Route("api/[controller]")]
[ApiController]
public class WarehouseController : ControllerBase
{
    private readonly IWarehouseService _warehouseService;

    public WarehouseController(IWarehouseService warehouseService)
    {
        _warehouseService = warehouseService;
    }

    [HttpPost]
    public async Task<IActionResult> AddProductToWarehouseAsync([FromBody] OrderDTO orderDto)
    {
        if (!await _warehouseService.DoesProductExistAsync(orderDto.IdProduct))
        {
            return BadRequest("Product doesnt exist");
        }

        if (orderDto.Amount <= 0)
        {
            return BadRequest("Invalid amount");
        }
        //TODO: find a way to parse that string to date and check if date is older than date in body
        if (!await _warehouseService.DoesOrderExistAsync(orderDto.IdProduct, orderDto.Amount))
        {
            return BadRequest("No such order was made");
        }
        
        orderDto.CreatedAt = ParseTheWierdDateFormat(orderDto.CreatedAt);
        var orderId = await _warehouseService.GetIdOrderAsync(orderDto.IdProduct, orderDto.Amount, orderDto.CreatedAt);
        
        if (orderId < 0)
        {
            return BadRequest("No such order was made");
        }

        if (await _warehouseService.WasOrderRealizedAsync(orderId))
        {
            return BadRequest("Order was already realized");
        }
        
        return Ok();
    }
    public static string ParseTheWierdDateFormat(string date)
    {
        date = date.Replace("T", " ");
        date = date.Replace("Z", "");
        date.ToCharArray()[date.Length - 5] = '.';
        return date;
    }
}

