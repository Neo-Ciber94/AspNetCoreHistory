using AspNetCoreHistory.Common;
using System.ComponentModel.DataAnnotations.Schema;

namespace AspNetCoreHistory.Models;

public class Product : IEntity, IHasHistory<Product, ProductHistory>
{
    public int Id { get; set; }
    public string Name { get; set; } = default!;
    public string? Description { get; set; }

    [Column(TypeName = "decimal(18,4)")]
    public decimal Price { get; set; }
}
