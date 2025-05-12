using System.Data.Common;
using Microsoft.Data.SqlClient;
using Tutorial9.Model.DTOs;

namespace Tutorial9.Services;

public class WarehouseService : IWarehouseService
{
    private readonly string _connectionString = "Server=(localdb)\\MSSQLLocalDB;Initial Catalog=apbd;Integrated Security=True;Connect Timeout=30;Encrypt=False;Trust Server Certificate=False;Application Intent=ReadWrite;Multi Subnet Failover=False";
    
    public async Task<bool> DoesProductExistAsync (int idProduct)
    {
        var quantity = 0;
        
        var command = "SELECT COUNT(1) FROM Product WHERE IdProduct = @idProduct";
        
        await using (SqlConnection conn = new SqlConnection(_connectionString))
        await using (SqlCommand cmd = new SqlCommand(command, conn))
        {
            cmd.Parameters.AddWithValue("@idProduct", idProduct);
            await conn.OpenAsync();
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
        
        await using (SqlConnection conn = new SqlConnection(_connectionString))
        await using (SqlCommand cmd = new SqlCommand(command, conn))
        {
            cmd.Parameters.AddWithValue("@idProduct", idProduct);
            cmd.Parameters.AddWithValue("@amount", amount);
            await conn.OpenAsync();
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
        
        await using (SqlConnection conn = new SqlConnection(_connectionString))
        await using (SqlCommand cmd = new SqlCommand(command, conn))
        {
            cmd.Parameters.AddWithValue("@idOrder", idOrder);
            await conn.OpenAsync();
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
        
        var command = "SELECT IdOrder FROM [Order] WHERE IdProduct = @idProduct and Amount = @amount and CreatedAt < @createdAt";
        
        await using (SqlConnection conn = new SqlConnection(_connectionString))
        await using (SqlCommand cmd = new SqlCommand(command, conn))
        {
            cmd.Parameters.AddWithValue("@idProduct", idProduct);
            cmd.Parameters.AddWithValue("@amount", amount);
            cmd.Parameters.AddWithValue("@createdAt", createdAt);
            await conn.OpenAsync();
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

    public async Task UpdateFulfillmentTimeAsync(int idOrder)
    {
        string fulfilledAt = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.sss");
        
        await using SqlConnection connection = new SqlConnection(_connectionString);
        await using SqlCommand command = new SqlCommand();
        command.Connection = connection;
        await connection.OpenAsync();
        
        DbTransaction transaction = await connection.BeginTransactionAsync();
        command.Transaction = transaction as SqlTransaction;

        try
        {
            command.CommandText = "Update [Order] set FulfilledAt = @fulfilledAt where IdOrder = @idOrder";
            command.Parameters.AddWithValue("@fulfilledAt", fulfilledAt);
            command.Parameters.AddWithValue("@idOrder", idOrder);
            
            await command.ExecuteNonQueryAsync();
            command.Parameters.Clear();
            
            await transaction.CommitAsync();
        }
        catch (Exception)
        {
            transaction.Rollback();
            throw;
        }
    }

    public async Task<decimal> GetProductPriceAsync(int idProduct)
    {
        decimal productPrice = 0;
        var command = "SELECT Price FROM Product WHERE IdProduct = @idProduct";
        await using (SqlConnection conn = new SqlConnection(_connectionString))
        await using (SqlCommand cmd = new SqlCommand(command, conn))
        {
            cmd.Parameters.AddWithValue("@idProduct", idProduct);
            await conn.OpenAsync();
            using (var reader = await cmd.ExecuteReaderAsync())
            {
                while (await reader.ReadAsync())
                {
                    productPrice = reader.GetDecimal(0);
                }
            }
        }
        return productPrice;
        
    }

    public async Task<int> InsertOrderIntoWarehouseAsync(OrderDTO orderDto, decimal price)
    {
        string createdAt = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.sss");
        int id = 0;
        await using SqlConnection connection = new SqlConnection(_connectionString);
        await using SqlCommand command = new SqlCommand();
        command.Connection = connection;
        await connection.OpenAsync();
        
        DbTransaction transaction = await connection.BeginTransactionAsync();
        command.Transaction = transaction as SqlTransaction;

        try
        {
            command.CommandText = "Insert into Product_Warehouse (IdWarehouse, IdProduct, IdOrder, Amount, Price, CreatedAt) Values (@idWarehouse, @idProduct, @idOrder, @amount, @price, @createdAt)";
            command.Parameters.AddWithValue("@idWarehouse", orderDto.IdWarehouse);
            command.Parameters.AddWithValue("@idProduct", orderDto.IdProduct);
            command.Parameters.AddWithValue("@idOrder", orderDto.IdOrder);
            command.Parameters.AddWithValue("@amount", orderDto.Amount);
            command.Parameters.AddWithValue("@price", price * orderDto.Amount);
            command.Parameters.AddWithValue("@createdAt", createdAt);
            
            await command.ExecuteNonQueryAsync();
            command.Parameters.Clear();
            
            command.CommandText = "Select @@Identity";
            using (var reader = await command.ExecuteReaderAsync())
            {
                while (await reader.ReadAsync())
                {
                    // also apparently @@identity is a decimal if I Select it like that
                    // had to cast it to integer for this to work
                    id = (int) reader.GetDecimal(0);
                }
            }
            await transaction.CommitAsync();
            return id;
        }
        catch (Exception)
        {
            await transaction.RollbackAsync();
            throw;
        }
    }
}