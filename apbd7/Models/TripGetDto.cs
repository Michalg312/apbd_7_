namespace apbd7.Models;

public class TripGetDto
{
    public int Id { get; set; }
    public string Name { get; set; }
    public string Description { get; set; }
    public DateTime DateFrom { get; set; }
    public DateTime DateTo { get; set; }
    public int MaxPeople { get; set; }
    public List<string> Country { get; set; } = new List<string>();
    public int RegisteredAt { get; set; }
    public int ?Paymentdate { get; set; }
    
}