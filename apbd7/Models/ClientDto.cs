namespace apbd7.Models;

public class ClientDto
{
    public string FirstName { get; set; }
    public string LastName { get; set; }
    public string Email { get; set; }
    public string Telephone { get; set; }
    public string Pesel { get; set; }
    

    public List<TripGetDto> lista{ get; set; } =new List<TripGetDto>();
}