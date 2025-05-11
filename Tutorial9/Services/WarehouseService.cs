using Microsoft.Data.SqlClient;

namespace Tutorial9.Services;

public class WarehouseService : IWarehouseService
{
    private readonly string _connectionString = "Server=(localdb)\\MSSQLLocalDB;Initial Catalog=apbd;Integrated Security=True;Connect Timeout=30;Encrypt=False;Trust Server Certificate=False;Application Intent=ReadWrite;Multi Subnet Failover=False";
    
    public async Task<bool> DoesProductExistAsync (int idProduct)
    {
        var quantity = 0;
        
        var command = "SELECT COUNT(1) FROM Product WHERE IdProduct = @idProduct";
        
        using (SqlConnection conn = new SqlConnection(_connectionString))
        using (SqlCommand cmd = new SqlCommand(command, conn))
        {
            cmd.Parameters.AddWithValue("@idProduct", idProduct);
            conn.Open();
            using (var reader = await cmd.ExecuteReaderAsync())
            {
                while (await reader.ReadAsync())
                {
                    quantity = reader.GetInt32(0);
                }
            }
        }
        return quantity > 0;
    }

    public async Task<bool> DoesOrderExistAsync(int idProduct, int amount)
    {
        var quantity = 0;
        
        var command = "SELECT COUNT(1) FROM [Order] WHERE IdProduct = @idProduct and Amount = @amount";
        
        using (SqlConnection conn = new SqlConnection(_connectionString))
        using (SqlCommand cmd = new SqlCommand(command, conn))
        {
            cmd.Parameters.AddWithValue("@idProduct", idProduct);
            cmd.Parameters.AddWithValue("@amount", amount);
            conn.Open();
            using (var reader = await cmd.ExecuteReaderAsync())
            {
                while (await reader.ReadAsync())
                {
                    quantity = reader.GetInt32(0);
                }
            }
        }
        return quantity > 0;
    }

    public async Task<bool> WasOrderRealizedAsync(int idOrder)
    {
        var quantity = 0;
        
        var command = "SELECT COUNT(1) FROM Product_Warehouse WHERE IdOrder = @idOrder";
        
        using (SqlConnection conn = new SqlConnection(_connectionString))
        using (SqlCommand cmd = new SqlCommand(command, conn))
        {
            cmd.Parameters.AddWithValue("@idOrder", idOrder);
            conn.Open();
            using (var reader = await cmd.ExecuteReaderAsync())
            {
                while (await reader.ReadAsync())
                {
                    quantity = reader.GetInt32(0);
                }
            }
        }
        return quantity > 0;
    }

    public async Task<int> GetIdOrderAsync(int idProduct, int amount, string createdAt)
    {
        int idOrder = -1;
        
        var command = "SELECT IdOrder FROM [Order] WHERE IdProduct = @idProduct and Amount = @amount and CreatedAt = @createdAt";
        
        using (SqlConnection conn = new SqlConnection(_connectionString))
        using (SqlCommand cmd = new SqlCommand(command, conn))
        {
            cmd.Parameters.AddWithValue("@idProduct", idProduct);
            cmd.Parameters.AddWithValue("@amount", amount);
            cmd.Parameters.AddWithValue("@createdAt", createdAt);
            conn.Open();
            using (var reader = await cmd.ExecuteReaderAsync())
            {
                while (await reader.ReadAsync())
                {
                    idOrder = reader.GetInt32(0);
                }
            }
        }
        
        return idOrder;
    }
}