using apbd7.Services;
using Microsoft.AspNetCore.Mvc;

namespace apbd7.Controllers;
[ApiController]
[Route("api/trips")]
public class TripController(ITripService service) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> GetClients()
    {
      
        return Ok(await service.GetAllTripsAsync());
    }
}