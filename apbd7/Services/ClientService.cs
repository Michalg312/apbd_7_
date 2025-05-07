using apbd7.Models;
using apbd7.Repository;
using Microsoft.AspNetCore.Mvc;

namespace apbd7.Services;

public class ClientService: IClientService
{
    private readonly IClientRepository _clientRepository;

    public ClientService( IClientRepository clientRepository)
    {
        _clientRepository = clientRepository;
    }
    public Task<IEnumerable<ClientDto>> GetAllUserTripsAsync(int id)
    {
        var users = _clientRepository.GetAllUserTripsAsync(id);
        return users;
        
    }

    public Task<Client>  PostClient(ClientCreateDto client)
    {
        var inf = _clientRepository.PostClient(client);
        return inf;
    }

    public string RegisterClientToTrip(int id, int tripId)
    {
        var result = _clientRepository.RegisterClientToTrip(id, tripId);
        return result;

    }

    public string DeleteClientToTrip(int id, int tripId)
    {
        var result = _clientRepository.DeleteClientToTrip(id, tripId);
        return result;
    }
}