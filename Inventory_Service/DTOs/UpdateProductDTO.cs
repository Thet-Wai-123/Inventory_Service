using Inventory_Service.Models;

namespace Inventory_Service.DTOs
{
    public record class UpdateProductDTO :IEquatable<Product>
    {
        public required int Id
        {
            get; set;
        }
        public required string Name
        {
            get; set;
        }

        public string? Description
        {
            get; set;
        }

        public required decimal Price
        {
            get; set;
        }

        public required int PostedBy
        {
            get; set;
        }

        public bool Equals(Product product)
        {
            if (
                Id == product.Id
                && Name == product.Name
                && Description == product.Description
                && Price == product.Price
                && PostedBy == product.Postedby
            )
            {
                return true;
            }
            else
            {
                return false;
            }
        }
    }
}
