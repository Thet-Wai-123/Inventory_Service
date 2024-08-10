using FluentAssertions;
using Inventory_Service.DTOs;
using Inventory_Service.Models;
using Inventory_Service.WorkerService;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using System.Text.Json;

namespace Inventory_Service_Testing
{
    public class DatabaseTesting :IClassFixture<WebApplicationFactory<Program>>
    {
        private readonly WebApplicationFactory<Program> _factory;
        private readonly HttpClient _client;
        public DatabaseTesting(WebApplicationFactory<Program> factory)
        {
            _factory = factory;
            _client = factory.CreateClient();
        }

        [Fact]
        public void ConnectionToDatabase()
        {
            using (var scope = _factory.Services.CreateScope())
            {
                InventoryDbContext _dbContext = scope.ServiceProvider.GetRequiredService<InventoryDbContext>();
                var resut = _dbContext.Database.CanConnect();
                Assert.True(resut);
            }
        }
        [Fact]
        public async Task MessagingQueue_HandlingCreateNewProduct_Async()
        {
            using (var scope = _factory.Services.CreateScope())
            {
                //Arrange
                InventoryDbContext _dbContext = scope.ServiceProvider.GetRequiredService<InventoryDbContext>();

                //Act
                CreateProductDTO productToCreate = new()
                {
                    Name = "CreateTest",
                    Description = "Test",
                    Price = 10,
                    PostedBy = 1
                };
                await MessagingQueues.HandleQueueTask("CREATE", JsonSerializer.Serialize(productToCreate), _dbContext);

                //Assert
                var returnedProduct = _dbContext.Products.FirstOrDefault(p => p.Name == "CreateTest");
                CreateProductDTO returnedProductWithoutId = new()
                {
                    Name = returnedProduct.Name,
                    Description = returnedProduct.Description,
                    Price = returnedProduct.Price,
                    PostedBy = (int)returnedProduct.Postedby
                };
                returnedProductWithoutId.Should().BeEquivalentTo(productToCreate);

                //Clean up
                _dbContext.Remove(returnedProduct);
                await _dbContext.SaveChangesAsync();
            }
        }

        [Fact]
        public async Task MessagingQueue_HandlingUpdateProduct_Async()
        {
            using (var scope = _factory.Services.CreateScope())
            {
                //Arrange
                InventoryDbContext _dbContext = scope.ServiceProvider.GetRequiredService<InventoryDbContext>();
                await _dbContext.Products.AddAsync(new Product
                {
                    Name = "OldProduct",
                    Description = "Test",
                    Price = 10,
                    Postedby = 1
                });
                await _dbContext.SaveChangesAsync();

                //Act
                Product productToUpdate = _dbContext.Products.FirstOrDefault(p => p.Name == "OldProduct");
                UpdateProductDTO newProduct = new()
                {
                    Id = productToUpdate.Id,
                    Name = "NewProduct",
                    Description = "NewTest",
                    Price = 20,
                    PostedBy = 1
                };
                await MessagingQueues.HandleQueueTask("UPDATE", JsonSerializer.Serialize(newProduct), _dbContext);

                //Assert
                var returnedProduct = _dbContext.Products.Find(productToUpdate.Id);
                Assert.True(newProduct.Equals(returnedProduct));

                //Clean up
                _dbContext.Remove(returnedProduct);
                await _dbContext.SaveChangesAsync();

            }
        }

        [Fact]
        public async Task MessagingQueue_HandlingDeleteProduct_Async()
        {
            using (var scope = _factory.Services.CreateScope())
            {
                //Arrange
                InventoryDbContext _dbContext = scope.ServiceProvider.GetRequiredService<InventoryDbContext>();
                await _dbContext.Products.AddAsync(new Product
                {
                    Name = "ProductToDelete",
                    Description = "Test",
                    Price = 10,
                    Postedby = 1
                });
                await _dbContext.SaveChangesAsync();

                //Act
                Product productToDelete = _dbContext.Products.FirstOrDefault(p => p.Name == "ProductToDelete");
                DeleteProductDTO target = new()
                {
                    Id = productToDelete.Id,
                    PostedBy = productToDelete.Postedby.Value
                };
                await MessagingQueues.HandleQueueTask("DELETE", JsonSerializer.Serialize(target), _dbContext);

                //Assert
                var returnedProduct = _dbContext.Products.Find(productToDelete.Id);
                Assert.Null(returnedProduct);
            }
        }
    }
}