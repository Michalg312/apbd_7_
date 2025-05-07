using apbd7.Models;

namespace apbd7.Repository;

public interface ITripsRepository
{
    public  Task<IEnumerable<TripGetDto>> GetAllTripsAsync();
}