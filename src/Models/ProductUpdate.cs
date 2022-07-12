namespace AspNetCoreHistory.Models;

public class ProductUpdate
{
    public int Id { get; set; }
    public string? Name { get; set; } = default!;
    public decimal? Price { get; set; }
}
