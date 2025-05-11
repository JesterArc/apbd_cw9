namespace Tutorial9.Model.DTOs;

public class OrderDTO
{
    public required int IdProduct { get; set; }
    public required int IdWarehouse { get; set; }
    public required int Amount { get; set; }
    public required string CreatedAt { get; set; }
    public string? FulfilledAt { get; set; }
}