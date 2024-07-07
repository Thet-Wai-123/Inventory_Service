using Inventory_Service.Models;
using System.Xml.Linq;

namespace Inventory_Service.Database_Operations
{
    public class Database_Operations
    {
        public static async Task CreateProduct(Product product, InventoryDbContext dbContext)
        {
            Product newProduct = new Product()
            {
                Name= product.Name,
                Description= product.Description,
                Price= product.Price
            };
        await dbContext.Products.AddAsync(product);
        await dbContext.SaveChangesAsync();
    }
    }
}
