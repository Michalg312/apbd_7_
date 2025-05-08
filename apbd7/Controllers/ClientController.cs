using apbd7.Models;
using apbd7.Repository;
using apbd7.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;

namespace apbd7.Controllers;

[ApiController]
[Route("api/clients")]
public class ClientController(IClientService iClientService,IConfiguration configuration ) : ControllerBase
{
    
    [HttpGet("check")]
    public IActionResult GetAllClients()
    {
        var clients = new List<Client>();

        using (var connection = new SqlConnection(  configuration.GetConnectionString("Default")))
        using (var command = new SqlCommand(@"
        SELECT IdClient, FirstName, LastName, Email, Telephone, Pesel
        FROM Client", connection))
        {
            connection.Open();
            using (var reader = command.ExecuteReader())
            {
                while (reader.Read())
                {
                    var client = new Client
                    {
                        Id = reader.GetInt32(0),
                        FirstName = reader.GetString(1),
                        LastName = reader.GetString(2),
                        Email = reader.GetString(3),
                        Telephone = reader.IsDBNull(4) ? null : reader.GetString(4),
                        Pesel = reader.IsDBNull(5) ? null : reader.GetString(5)
                    };
                    clients.Add(client);
                }
            }
        }

        return Ok(clients);
    }
    
    [HttpGet("list")]
    public IActionResult GetAllClientTrips()
    {
        var clientTrips = new List<object>();

        using (var connection = new SqlConnection(configuration.GetConnectionString("Default")))
        using (var command = new SqlCommand(@"
        SELECT IdClient, IdTrip, RegisteredAt, PaymentDate
        FROM Client_Trip", connection))
        {
            connection.Open();
            using (var reader = command.ExecuteReader())
            {
                while (reader.Read())
                {
                    clientTrips.Add(new
                    {
                        IdClient = reader.GetInt32(0),
                        IdTrip = reader.GetInt32(1),
                        RegisteredAt = reader.GetInt32(2),
                        PaymentDate = reader.IsDBNull(3) ? (int?)null : reader.GetInt32(3) 
                    });
                }
            }
        }

        return Ok(clientTrips);
    }

 
    [HttpGet("{id}/trips")]
    public async Task<IActionResult> GetClientTripsByIdAsync(int id)
    {
        return Ok(await iClientService.GetAllUserTripsAsync(id));
        // return Ok(await iTripService.);
    }
    
    
    
    [HttpPost]
    public  IActionResult PostClient([FromBody] ClientCreateDto client)
    {
        var c = iClientService.PostClient(client);
        return Created($"client/{c.Id}", client);
        // return Ok(iClientService.PostClient(client));

    }
    
    [HttpPut("{id}/trips/{tripId}")]
    public async Task<IActionResult> RegisterClientToTrip(int id, int tripId)
    {
        string result = iClientService.RegisterClientToTrip(id, tripId).Result;

        return result switch
        {
            "NOT_FOUND_CLIENT" => NotFound("Klient nie istnieje."),
            "NOT_FOUND_TRIP" => NotFound("Wycieczka nie istnieje."),
            "ALREADY_REGISTERED" => BadRequest("Klient już zapisany."),
            "MAX_REACHED" => BadRequest("Osiągnięto maksymalną liczbę uczestników."),
            "SUCCESS" => Ok("Klient został zarejestrowany na wycieczkę."),
            _ => StatusCode(500, "Nieznany błąd.")
        };
    }
    
    [HttpDelete("{id}/trips/{tripId}")]
    public async Task<IActionResult> DeleteClientToTrip(int id, int tripId)
    {
        string result =  iClientService.DeleteClientToTrip(id, tripId).Result;

        return result switch
        {
            "Rejestracja nie istnieje." => NotFound("Rejestracja nie istnieje."),
            "Rejestracja została usunięta." =>Ok("Rejestracja usunieta"),
           _ => StatusCode(500, "Nieznany błąd.")
        };
    }

    
}