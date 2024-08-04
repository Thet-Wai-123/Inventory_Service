using System.ComponentModel.DataAnnotations;

namespace MarketPlace_API_Gateway.DTOs.Inventory_Services
{
    public class QueryProductDTO
    {
        [Required]
        public string keyword { get; set; }
        public decimal? minPrice { get; set; }
        public decimal? maxPrice { get; set; }
    }
}
