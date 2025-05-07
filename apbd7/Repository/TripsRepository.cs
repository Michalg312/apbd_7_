using apbd7.Models;
using Microsoft.Data.SqlClient;

namespace apbd7.Repository;

public class TripsRepository: ITripsRepository
{
    private readonly string _connectionString;

    public TripsRepository(IConfiguration configuration)
    {
        _connectionString = configuration.GetConnectionString("Default");
    }

    public async Task<IEnumerable<TripGetDto>> GetAllTripsAsync()
    {
        var result = new List<TripGetDto>();
        // var connectionString = configuration.GetConnectionString("Default");
        await using var connection = new SqlConnection(_connectionString);


        // var sql = "select Idtrip, name,description ,dateFrom,dateTo,maxpeople from trip";
        var sql =
            "select T.idTrip,T.name, T.description, T.datefrom, T.dateTo, T.maxpeople, C.Name  from  Trip as T inner join Country_Trip as CT on T.IdTrip= CT.IdTrip \ninner join Country as C on CT.IdCountry=C.IdCountry order by T.IdTrip asc";


        await using var command = new SqlCommand(sql, connection);
        await connection.OpenAsync();
        await using var reader = await command.ExecuteReaderAsync();

        var tripDict = new Dictionary<int, TripGetDto>();

        while (await reader.ReadAsync())
        {
            var idTrip = reader.GetInt32(0);
            var countryName = reader.GetString(6);

            if (!tripDict.TryGetValue(idTrip, out var trip))
            {
                trip = new TripGetDto()
                {
                    Id = idTrip,
                    Name = reader.GetString(1),
                    Description = reader.GetString(2),
                    DateFrom = reader.GetDateTime(3),
                    DateTo = reader.GetDateTime(4),
                    MaxPeople = reader.GetInt32(5),
                    Country = new List<string>()
                };
                tripDict.Add(idTrip, trip);
            }

            trip.Country.Add(countryName);
        }

        return tripDict.Values;
    }
}