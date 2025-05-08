using apbd7.Models;
using Microsoft.AspNetCore.Mvc;

namespace apbd7.Repository;

public interface IClientService
{
    public  Task<IEnumerable<ClientDto>> GetAllUserTripsAsync(int id);
    public Task<Client> PostClient([FromBody] ClientCreateDto client);
    public Task<string> RegisterClientToTrip(int id, int tripId);
    public Task<string> DeleteClientToTrip(int id, int tripId);
}