using System.Data.Common;
using Inventory_Service.Models;
using MarketPlace_API_Gateway.DTOs.Inventory_Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Npgsql;

namespace Inventory_Service.Endpoints
{
    public static class Endpoints
    {
        public static void MapInventoryEndpoints(this WebApplication app)
        {
            app.MapGet(
                "/",
                async (InventoryDbContext dbContext) =>
                {
                    var products = await dbContext.Products.ToListAsync();
                    return Results.Ok(products);
                }
            );
            app.MapGet(
                "/{id}",
                async (InventoryDbContext dbContext, int id) =>
                {
                    var product = await dbContext.Products.FindAsync(id);
                    if (product == null)
                    {
                        return Results.NotFound();
                    }
                    return Results.Ok(product);
                }
            );

            app.MapPost(
                "/query",
                async ([FromBody] QueryProductDTO productToQuery, InventoryDbContext dbContext) =>
                {
                    Console.Write(productToQuery);
                    if (productToQuery.minPrice == null)
                    {
                        productToQuery.minPrice = 0;
                    }
                    if (productToQuery.maxPrice == null)
                    {
                        //Might opt to set a limit in appsettings.json later
                        productToQuery.maxPrice = 999999;
                    }

                    DbParameter searchKeyword = new NpgsqlParameter(
                        "searchKeyword",
                        $"%{productToQuery.keyword}%"
                    );
                    var foundProducts = await dbContext
                        .Products.FromSqlRaw(
                            "SELECT * FROM Products WHERE name ILIKE @searchKeyword and price >= @p1 and price <= @p2",
                            searchKeyword,
                            productToQuery.minPrice,
                            productToQuery.maxPrice
                        )
                        .ToListAsync();
                    return Results.Ok(foundProducts);
                }
            );
        }
    }
}
