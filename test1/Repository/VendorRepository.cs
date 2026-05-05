namespace test1.Repository;

public class VendorRepository : IVendorRepository
{
    private readonly string _connectionString;

    public RentalRepository(IConfiguration configuration)
    {
        _connectionString = configuration.GetConnectionString("Default")
            ?? throw new InvalidOperationException("Missing 'Default' connection string.");
    }
    public async Task<CustomerRentalsResponse?> GetCustomerRentalsAsync(int customerId)
    {
        await using var connection = new SqlConnection(_connectionString);
        await connection.OpenAsync();

        string firstName;
        string lastName;

        await using (var customerCommand = new SqlCommand(
            "SELECT first_name, last_name FROM Customer WHERE customer_id = @customerId;",
            connection))
        {
            customerCommand.Parameters.AddWithValue("@customerId", customerId);

            await using var customerReader = await customerCommand.ExecuteReaderAsync();
            if (!await customerReader.ReadAsync())
            {
                return null;
            }

            firstName = customerReader.GetString(0);
            lastName = customerReader.GetString(1);
        }

        var rentalsById = new Dictionary<int, RentalResponse>();

        await using (var rentalsCommand = new SqlCommand(@"
            SELECT  r.rental_id,
                    r.rental_date,
                    r.return_date,
                    s.name           AS status_name,
                    m.title          AS movie_title,
                    ri.price_at_rental
            FROM    Rental       r
            JOIN    Status       s  ON s.status_id = r.status_id
            LEFT JOIN Rental_Item ri ON ri.rental_id = r.rental_id
            LEFT JOIN Movie       m  ON m.movie_id  = ri.movie_id
            WHERE   r.customer_id = @customerId
            ORDER BY r.rental_id, m.title;", connection))
        {
            rentalsCommand.Parameters.AddWithValue("@customerId", customerId);

            await using var reader = await rentalsCommand.ExecuteReaderAsync();
            while (await reader.ReadAsync())
            {
                var rentalId = reader.GetInt32(0);

                if (!rentalsById.TryGetValue(rentalId, out var rental))
                {
                    rental = new RentalResponse
                    {
                        Id = rentalId,
                        RentalDate = reader.GetDateTime(1),
                        ReturnDate = reader.IsDBNull(2) ? null : reader.GetDateTime(2),
                        Status = reader.GetString(3),
                        Movies = new List<RentalMovieResponse>()
                    };
                    rentalsById.Add(rentalId, rental);
                }

                if (!reader.IsDBNull(4))
                {
                    rental.Movies.Add(new RentalMovieResponse
                    {
                        Title = reader.GetString(4),
                        PriceAtRental = reader.GetDecimal(5)
                    });
                }
            }
        }

        return new CustomerRentalsResponse
        {
            FirstName = firstName,
            LastName = lastName,
            Rentals = rentalsById.Values.ToList()
        };
    }

    public async Task CreateRentalAsync(int customerId, CreateRentalRequest request)
    {
        await using var connection = new SqlConnection(_connectionString);
        await connection.OpenAsync();

        await using var transaction = (SqlTransaction)await connection.BeginTransactionAsync();

        try
        {
            await using (var customerCheck = new SqlCommand(
                "SELECT 1 FROM Customer WHERE customer_id = @customerId;",
                connection,
                transaction))
            {
                customerCheck.Parameters.AddWithValue("@customerId", customerId);
                var exists = await customerCheck.ExecuteScalarAsync();
                if (exists is null)
                {
                    throw new NotFoundException($"Customer with id {customerId} was not found.");
                }
            }

            var movieIds = new List<int>();
            foreach (var movie in request.Movies)
            {
                await using var movieCommand = new SqlCommand(
                    "SELECT movie_id FROM Movie WHERE title = @title;",
                    connection,
                    transaction);
                movieCommand.Parameters.AddWithValue("@title", movie.Title);

                var movieId = await movieCommand.ExecuteScalarAsync();
                if (movieId is null)
                {
                    throw new NotFoundException($"Movie with title '{movie.Title}' was not found.");
                }

                movieIds.Add((int)movieId);
            }

            int statusId;
            await using (var statusCommand = new SqlCommand(
                "SELECT status_id FROM Status WHERE name = @name;",
                connection,
                transaction))
            {
                statusCommand.Parameters.AddWithValue("@name", "Rented");
                var statusResult = await statusCommand.ExecuteScalarAsync();
                if (statusResult is null)
                {
                    throw new InvalidOperationException("Status 'Rented' is not configured in the database.");
                }
                statusId = (int)statusResult;
            }

            await using (var rentalCommand = new SqlCommand(@"
                SET IDENTITY_INSERT Rental ON;
                INSERT INTO Rental (rental_id, rental_date, return_date, customer_id, status_id)
                VALUES (@rentalId, @rentalDate, NULL, @customerId, @statusId);
                SET IDENTITY_INSERT Rental OFF;", connection, transaction))
            {
                rentalCommand.Parameters.AddWithValue("@rentalId", request.Id);
                rentalCommand.Parameters.AddWithValue("@rentalDate", request.RentalDate);
                rentalCommand.Parameters.AddWithValue("@customerId", customerId);
                rentalCommand.Parameters.AddWithValue("@statusId", statusId);

                await rentalCommand.ExecuteNonQueryAsync();
            }

            for (var i = 0; i < request.Movies.Count; i++)
            {
                await using var itemCommand = new SqlCommand(
                    @"INSERT INTO Rental_Item (rental_id, movie_id, price_at_rental)
                      VALUES (@rentalId, @movieId, @priceAtRental);",
                    connection,
                    transaction);
                itemCommand.Parameters.AddWithValue("@rentalId", request.Id);
                itemCommand.Parameters.AddWithValue("@movieId", movieIds[i]);
                itemCommand.Parameters.AddWithValue("@priceAtRental", request.Movies[i].RentalPrice);

                await itemCommand.ExecuteNonQueryAsync();
            }

            await transaction.CommitAsync();
        }
        catch
        {
            await transaction.RollbackAsync();
            throw;
        }
    }

    public async Task<CustomerDto?> GetCustomerAsync(int customerId)
    {
        await using var connection = new SqlConnection(_connectionString);
        await connection.OpenAsync();

        await using var command = new SqlCommand(
            "SELECT customer_id, first_name, last_name FROM Customer WHERE customer_id = @customerId;",
            connection);
        command.Parameters.AddWithValue("@customerId", customerId);

        await using var reader = await command.ExecuteReaderAsync();
        if (!await reader.ReadAsync())
        {
            return null;
        }

        return new CustomerDto
        {
            Id = reader.GetInt32(0),
            FirstName = reader.GetString(1),
            LastName = reader.GetString(2)
        };
    }

    public async Task AddCustomerAsync(CustomerDto customer)
    {
        await using var connection = new SqlConnection(_connectionString);
        await connection.OpenAsync();

        await using var command = new SqlCommand(
            "INSERT INTO Customer (first_name, last_name) VALUES (@firstName, @lastName);",
            connection);
        command.Parameters.AddWithValue("@firstName", customer.FirstName);
        command.Parameters.AddWithValue("@lastName", customer.LastName);

        await command.ExecuteNonQueryAsync();
    }

    public async Task UpdateCustomerAsync(int customerId, CustomerDto customer)
    {
        await using var connection = new SqlConnection(_connectionString);
        await connection.OpenAsync();

        await using var command = new SqlCommand(
            "UPDATE Customer SET first_name = @firstName, last_name = @lastName WHERE customer_id = @customerId;",
            connection);
        command.Parameters.AddWithValue("@customerId", customerId);
        command.Parameters.AddWithValue("@firstName", customer.FirstName);
        command.Parameters.AddWithValue("@lastName", customer.LastName);

        await command.ExecuteNonQueryAsync();
    }

    public async Task DeleteCustomerAsync(int customerId)
    {
        await using var connection = new SqlConnection(_connectionString);
        await connection.OpenAsync();

        await using var command = new SqlCommand(
            "DELETE FROM Customer WHERE customer_id = @customerId;",
            connection);
        command.Parameters.AddWithValue("@customerId", customerId);

        await command.ExecuteNonQueryAsync();
    }
}