using System.ComponentModel.DataAnnotations;

namespace Inventory_Service.DTOs
{
    public record class CreateProductDTO
    {
        public required string Name { get; set; }

        public string? Description { get; set; }

        public required decimal Price { get; set; }
        public required int PostedBy { get; set; }
    }
}
