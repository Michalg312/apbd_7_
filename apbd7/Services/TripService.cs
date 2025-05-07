using apbd7.Models;
using apbd7.Repository;

namespace apbd7.Services;

public class TripService : ITripService
{
  
    private readonly ITripsRepository _tripsRepository;

    public TripService( ITripsRepository tripsRepository)
    {
        _tripsRepository = tripsRepository;
    }

    public Task<IEnumerable<TripGetDto>> GetAllTripsAsync()
    {
        var trips = _tripsRepository.GetAllTripsAsync();
        return trips;
    }
}