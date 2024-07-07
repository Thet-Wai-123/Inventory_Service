namespace Marketplace_API_Gateway.DTOs
{
    public record class ProductSummaryDTO
    {
        public required string Name { get; set; }

        public string? Description { get; set; }

        public required decimal Price { get; set; }

        public string? PostedByName { get; set; }

        public string? PostedByID { get; set; }
    }
}
