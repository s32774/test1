using Microsoft.Data.SqlClient;
using test1.DTOs;
using test1.Exception;

namespace test1.Repository;

public class VendorRepository : IVendorRepository
{
    private readonly string _connectionString;

    public VendorRepository(IConfiguration configuration)
    {
        _connectionString = configuration.GetConnectionString("Default")
                            ?? throw new InvalidOperationException(
                                "Missing connection string.");
    }

    public async Task<VendorResponseDto?> GetVendorAsync(string code)
    {
        await using var connection = new SqlConnection(_connectionString);

        await connection.OpenAsync();

        VendorResponseDto? vendor = null;

        var sql = @"
SELECT  v.Code,
        v.Name,
        p.Id,
        p.Name,
        p.Description,
        p.StickerPrice,
        pt.Id,
        pt.Name,
        m.Id,
        m.Name,
        vp.Amount,
        vp.PricePerUnit
FROM Vendors v
LEFT JOIN VendorProducts vp ON vp.VendorCode = v.Code
LEFT JOIN Products p ON p.Id = vp.ProductId
LEFT JOIN ProductTypes pt ON pt.Id = p.ProductTypeId
LEFT JOIN Makers m ON m.Id = p.MakerId
WHERE v.Code = @code
ORDER BY p.Name;";

        await using var command = new SqlCommand(sql, connection);

        command.Parameters.AddWithValue("@code", code);

        await using var reader = await command.ExecuteReaderAsync();

        while (await reader.ReadAsync())
        {
            if (vendor is null)
            {
                vendor = new VendorResponseDto
                {
                    Code = reader.GetString(0),
                    Name = reader.GetString(1),
                    Products = []
                };
            }

            if (!reader.IsDBNull(2))
            {
                vendor.Products.Add(new ProductResponseDto
                {
                    Id = reader.GetInt32(2),
                    Name = reader.GetString(3),
                    Description = reader.IsDBNull(4) ? string.Empty : reader.GetString(4),
                    StrickerPrice = reader.GetDecimal(5),

                    ProductType = new ProductTypeDto
                    {
                        Id = reader.GetInt32(6),
                        Name = reader.GetString(7)
                    },

                    Maker = new MakerDto
                    {
                        Id = reader.GetInt32(8),
                        Name = reader.GetString(9)
                    },

                    VendorOffer = new VendorOfferDto
                    {
                        Amount = reader.GetInt32(10),
                        PricePerUnit = reader.GetDecimal(11)
                    }
                });
            }
        }

        return vendor;
    }

    public async Task AddVendorAsync(CreateVendorRequest request)
    {
        await using var connection = new SqlConnection(_connectionString);

        await connection.OpenAsync();

        await using var transaction =
            (SqlTransaction)await connection.BeginTransactionAsync();

        try
        {
            await using (var vendorCommand = new SqlCommand(@"
INSERT INTO Vendors(Code, Name)
VALUES (@code, @name);",
                connection,
                transaction))
            {
                vendorCommand.Parameters.AddWithValue("@code", request.Code);
                vendorCommand.Parameters.AddWithValue("@name", request.Name);

                await vendorCommand.ExecuteNonQueryAsync();
            }

            foreach (var product in request.Products)
            {
                await using var productCheck = new SqlCommand(@"
SELECT 1
FROM Products
WHERE Id = @productId;",
                    connection,
                    transaction);

                productCheck.Parameters.AddWithValue("@productId", product.Id);

                var productExists = await productCheck.ExecuteScalarAsync();

                if (productExists is null)
                {
                    throw new NotFoundException(
                        $"Product with id {product.Id} was not found.");
                }

                await using var productCommand = new SqlCommand(@"
INSERT INTO VendorProducts(ProductId, VendorCode, Amount, PricePerUnit)
VALUES (@productId, @vendorCode, @amount, @pricePerUnit);",
                    connection,
                    transaction);

                productCommand.Parameters.AddWithValue("@productId", product.Id);
                productCommand.Parameters.AddWithValue("@vendorCode", request.Code);
                productCommand.Parameters.AddWithValue("@amount", product.Amount);
                productCommand.Parameters.AddWithValue("@pricePerUnit", product.PricePerUnit);

                await productCommand.ExecuteNonQueryAsync();
            }

            await transaction.CommitAsync();
        }
        catch
        {
            await transaction.RollbackAsync();

            throw;
        }
    }
}