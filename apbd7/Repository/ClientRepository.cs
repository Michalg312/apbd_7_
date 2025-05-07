using apbd7.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;

namespace apbd7.Repository;

public class ClientRepository : IClientRepository
{
    private readonly string _connectionString;

    public ClientRepository(IConfiguration configuration)
    {
        _connectionString = configuration.GetConnectionString("Default");
    }

    public async Task<IEnumerable<ClientDto>> GetAllUserTripsAsync(int id)
    {
        var result = new List<TripGetDto>();
        await using var connection = new SqlConnection(_connectionString);

        var sql =
            @" SELECT C.IdClient, C.FirstName, C.LastName, C.Email, C.Telephone, C.Pesel,   T.IdTrip, T.Name, T.Description, T.DateFrom, T.DateTo, T.MaxPeople ,CT.PaymentDate,CT.RegisteredAt FROM Client AS C  JOIN Client_Trip CT ON C.IdClient = CT.IdClient   INNER JOIN Trip T ON CT.IdTrip = T.IdTrip    WHERE C.IdClient = @id";
        await using var command = new SqlCommand(sql, connection);
        command.Parameters.AddWithValue("@id", id); // 👈 bezpieczne przekazanie parametru
        await connection.OpenAsync();

        await using var reader = await command.ExecuteReaderAsync();

        var clientDict = new Dictionary<int, ClientDto>();

        while (await reader.ReadAsync())
        {
            var clientId = reader.GetInt32(0);

            if (!clientDict.TryGetValue(clientId, out var client))
            {
                client = new ClientDto
                {
                    FirstName = reader.GetString(1),
                    LastName = reader.GetString(2),
                    Email = reader.GetString(3),
                    Telephone = reader.IsDBNull(4) ? null : reader.GetString(4),
                    Pesel = reader.IsDBNull(5) ? null : reader.GetString(5),

                    lista = new List<TripGetDto>()
                };
                clientDict[clientId] = client;
            }

            var trip = new TripGetDto
            {
                Id = reader.GetInt32(6),
                Name = reader.GetString(7),
                Description = reader.IsDBNull(8) ? null : reader.GetString(8),
                DateFrom = reader.GetDateTime(9),
                DateTo = reader.GetDateTime(10),
                MaxPeople = reader.GetInt32(11),
                Paymentdate = reader.IsDBNull(12) ? null : reader.GetInt32(12),

                RegisteredAt = reader.GetInt32(13),
                Country = new List<string>()
            };

            client.lista.Add(trip);
        }

        return clientDict.Values;
    }

    public async Task<Client> PostClient(ClientCreateDto client)
    {
        if (string.IsNullOrWhiteSpace(client.FirstName) ||
            string.IsNullOrWhiteSpace(client.LastName) ||
            string.IsNullOrWhiteSpace(client.Email))
        {
            // return BadRequest("Wszystkie wymagane pola muszą być wypełnione.");
        }

        int newId;

        string sql =
            @"INSERT INTO Client (FirstName, LastName, Email, Telephone, Pesel) OUTPUT INSERTED.IdClient VALUES (@FirstName, @LastName, @Email, @Telephone, @Pesel)";
        await using var connection = new SqlConnection(_connectionString);
        await using var command = new SqlCommand(sql, connection);

        command.Parameters.AddWithValue("@FirstName", client.FirstName);
        command.Parameters.AddWithValue("@LastName", client.LastName);
        command.Parameters.AddWithValue("@Email", client.Email);
        command.Parameters.AddWithValue("@Telephone",
            string.IsNullOrEmpty(client.Telephone) ? DBNull.Value : client.Telephone);
        command.Parameters.AddWithValue("@Pesel", string.IsNullOrEmpty(client.Pesel) ? DBNull.Value : client.Pesel);

        await connection.OpenAsync();
        newId = Convert.ToInt32(await command.ExecuteScalarAsync());
        return new Client()
        {
            Id = newId,
            FirstName = client.FirstName,
            LastName = client.LastName,
            Email = client.Email,
            Telephone = client.Telephone,
            Pesel = client.Pesel
        };

        // return Created($"/api/clients/{newId}", new { IdClient = newId });
    }

   public string RegisterClientToTrip(int id, int tripId)
{
    using var connection = new SqlConnection(_connectionString);
    connection.Open();

    using (var checkClient = new SqlCommand("SELECT COUNT(*) FROM Client WHERE IdClient = @Id", connection))
    {
        checkClient.Parameters.AddWithValue("@Id", id);
        if ((int)checkClient.ExecuteScalar() == 0)
            return "NOT_FOUND_CLIENT";
    }

    using (var checkTrip = new SqlCommand("SELECT COUNT(*) FROM Trip WHERE IdTrip = @TripId", connection))
    {
        checkTrip.Parameters.AddWithValue("@TripId", tripId);
        if ((int)checkTrip.ExecuteScalar() == 0)
            return "NOT_FOUND_TRIP";
    }

    using (var checkDuplicate = new SqlCommand(
               "SELECT COUNT(*) FROM Client_Trip WHERE IdClient = @Id AND IdTrip = @TripId", connection))
    {
        checkDuplicate.Parameters.AddWithValue("@Id", id);
        checkDuplicate.Parameters.AddWithValue("@TripId", tripId);
        if ((int)checkDuplicate.ExecuteScalar() > 0)
            return "ALREADY_REGISTERED";
    }

    using (var countCmd = new SqlCommand(@"
        SELECT 
            (SELECT COUNT(*) FROM Client_Trip WHERE IdTrip = @TripId),
            (SELECT MaxPeople FROM Trip WHERE IdTrip = @TripId)", connection))
    {
        countCmd.Parameters.AddWithValue("@TripId", tripId);
        using var reader = countCmd.ExecuteReader();
        if (reader.Read())
        {
            int current = reader.GetInt32(0);
            int max = reader.GetInt32(1);
            if (current >= max)
                return "MAX_REACHED";
        }
    }

    using (var insert = new SqlCommand(@"
        INSERT INTO Client_Trip (IdClient, IdTrip, RegisteredAt)
        VALUES (@Id, @TripId, @Now)", connection))
    {
        insert.Parameters.AddWithValue("@Id", id);
        insert.Parameters.AddWithValue("@TripId", tripId);
        insert.Parameters.AddWithValue("@Now", 20250105);
        insert.ExecuteNonQuery();
    }

    return "SUCCESS";
}

public string DeleteClientToTrip(int id, int tripId)
{
    using var connection = new SqlConnection(_connectionString);
    connection.Open();

    // Sprawdź, czy rejestracja istnieje
    using (var checkCmd = new SqlCommand(
               "SELECT COUNT(*) FROM Client_Trip WHERE IdClient = @Id AND IdTrip = @TripId", connection))
    {
        checkCmd.Parameters.AddWithValue("@Id", id);
        checkCmd.Parameters.AddWithValue("@TripId", tripId);

        int exists = (int)checkCmd.ExecuteScalar();
        if (exists == 0)
            return ("Rejestracja nie istnieje.");
    }

    // Usuń rejestrację
    using (var deleteCmd = new SqlCommand(
               "DELETE FROM Client_Trip WHERE IdClient = @Id AND IdTrip = @TripId", connection))
    {
        deleteCmd.Parameters.AddWithValue("@Id", id);
        deleteCmd.Parameters.AddWithValue("@TripId", tripId);
        deleteCmd.ExecuteNonQuery();
    }

    return ("Rejestracja została usunięta.");}
}