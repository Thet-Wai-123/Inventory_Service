using System;
using System.Collections.Generic;

namespace Inventory_Service.Models;

public partial class Product
{
    public int Id { get; set; }

    public string Name { get; set; } = null!;

    public string? Description { get; set; }

    public decimal Price { get; set; }

    public int? Postedby { get; set; }
}
