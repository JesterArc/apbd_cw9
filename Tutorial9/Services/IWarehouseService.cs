using Tutorial9.Model.DTOs;

namespace Tutorial9.Services;

public interface IWarehouseService
{
    public Task<bool> DoesProductExistAsync (int idProduct);
    public Task<bool> DoesOrderExistAsync (int idProduct, int amount);
    public Task<bool> WasOrderRealizedAsync (int idOrder);
    public Task<int> GetIdOrderAsync (int idProduct, int amount, string createdAt);
    public Task UpdateFulfillmentTimeAsync(int idOrder);
    public Task<decimal> GetProductPriceAsync (int idProduct);
    public Task<int> InsertOrderIntoWarehouseAsync (OrderDTO orderDto, decimal price);
}