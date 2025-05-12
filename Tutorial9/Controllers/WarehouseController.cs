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
        if (!ValidateDate(orderDto.CreatedAt))
        {
            return BadRequest("Invalid datetime format - use yyyy-mm-ddThh:mm:ss.sssZ");
        }
        if (!await _warehouseService.DoesProductExistAsync(orderDto.IdProduct))
        {
            return NotFound("Product doesnt exist");
        }

        if (orderDto.Amount <= 0)
        {
            return BadRequest("Invalid amount");
        }

        if (!await _warehouseService.DoesOrderExistAsync(orderDto.IdProduct, orderDto.Amount))
        {
            return NotFound("No such order was made");
        }
        // returns -1 if no order was found
        orderDto.IdOrder = await _warehouseService.GetIdOrderAsync(orderDto.IdProduct, orderDto.Amount, orderDto.CreatedAt);
        
        if (orderDto.IdOrder < 0)
        {
            return NotFound("No such order was made");
        }

        if (await _warehouseService.WasOrderRealizedAsync(orderDto.IdOrder))
        {
            return BadRequest("Order was already realized");
        }

        try
        {
            await _warehouseService.UpdateFulfillmentTimeAsync(orderDto.IdOrder);
        }
        catch
        {
            return BadRequest("Internal error, could not update order fulfillment time");
        }
        var price = await _warehouseService.GetProductPriceAsync(orderDto.IdProduct);
        try
        {
            return Ok(await _warehouseService.InsertOrderIntoWarehouseAsync(orderDto, price));
        }
        catch
        {
            return BadRequest("Internal error, could not insert into Product_Warehouse");
        }
        
    }

    private static bool ValidateDate(string date)
    {
        try
        {
            if (date.Length != 24) return false;
            int year = Int32.Parse(date.Substring(0, 4));
            if (year < 1900) return false;
            if (date.Substring(4, 1) != "-") return false;
            int month = Int32.Parse(date.Substring(5, 2));
            if (date.Substring(7, 1) != "-") return false;
            if (month < 1 || month > 12) return false;
            int day = Int32.Parse(date.Substring(8, 2));
            if (day < 1) return false;
            switch (month)
            {
                case 4 or 6 or 9 or 11:
                    if (day > 30) return false;
                    break;
                case 2:
                    if (day > 29 || (day == 29 && year % 4 != 0)) return false;
                    break;
                default:
                    if (day > 31) return false;
                    break;
            }
            if (date.Substring(10, 1) != "T") return false;
            int hour = Int32.Parse(date.Substring(11, 2));
            if (hour < 1 || hour > 23) return false;
            if (date.Substring(13, 1) != ":") return false;
            int minutes = Int32.Parse(date.Substring(14, 2));
            if (minutes < 0 || minutes > 59) return false;
            if (date.Substring(16, 1) != ":") return false;
            int seconds = Int32.Parse(date.Substring(17, 2));
            if (seconds < 0 || seconds > 59) return false;
            if (date.Substring(19, 1) != ".") return false;
            int milliseconds = Int32.Parse(date.Substring(20, 3));
            if (milliseconds < 0) return false;
            return date.Substring(23, 1) == "Z";
        }
        catch (Exception)
        {
            return false;
        }
        
    }
}

