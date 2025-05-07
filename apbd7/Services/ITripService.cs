using apbd7.Models;

namespace apbd7.Services;

public interface ITripService
{
    public Task<IEnumerable<TripGetDto>> GetAllTripsAsync();
}