using Inventory_Service.Models;
using Microsoft.EntityFrameworkCore;

namespace Inventory_Service.Endpoints
{
    public static class Endpoints
    {
        public static void MapInventoryEndpoints(this WebApplication app)
        {
            app.MapGet("/products", async (InventoryDbContext dbContext) =>
            {
                var products = await dbContext.Products.ToListAsync();
                return Results.Ok(products);
            });
            app.MapGet("/products/{id}", async (InventoryDbContext dbContext, int id) =>
            {
                var product = await dbContext.Products.FindAsync(id);
                if (product == null)
                {
                    return Results.NotFound();
                }
                return Results.Ok(product);
            });
        }
    }
}
