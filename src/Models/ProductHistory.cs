using AspNetCoreHistory.Common;
using System.ComponentModel.DataAnnotations.Schema;

namespace AspNetCoreHistory.Models;

public class ProductHistory : IHistory<ProductHistory, Product>
{
    public long HistoryId { get; set; }
    public DateTime CreatedAt { get; set; }

    #region Parent Properties
    public int Id { get; set; }
    public string Name { get; set; } = default!;
    public string? Description { get; set; }

    [Column(TypeName = "decimal(18,4)")]
    public decimal Price { get; set; }
    #endregion
}
