using System.Text;
using System.Text.Json;
using Inventory_Service.DTOs;
using Inventory_Service.Models;
using Microsoft.EntityFrameworkCore;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace Inventory_Service.WorkerService
{
    public static class MessagingQueues
    {
        private static InventoryDbContext _dbContext;

        public static void StartConsumingQueue(InventoryDbContext dbContext)
        {
            _dbContext = dbContext;
            var factory = new ConnectionFactory { HostName = "localhost" };
            var connection = factory.CreateConnection();
            var channel = connection.CreateModel();
            channel.QueueDeclare(
                queue: "Inventory_queue",
                durable: true,
                exclusive: false,
                autoDelete: false,
                arguments: null
            );
            channel.BasicQos(prefetchSize: 0, prefetchCount: 1, global: false);
            var consumer = new EventingBasicConsumer(channel);
            consumer.Received += async (model, ea) =>
            {
                byte[] body = ea.Body.ToArray();
                var message = Encoding.UTF8.GetString(body);

                if (ea.BasicProperties.Headers.TryGetValue("Action", out var actionObj))
                {
                    string action = Encoding.UTF8.GetString(actionObj as byte[]);
                    await HandleQueueTask(action, message);
                }

                channel.BasicAck(deliveryTag: ea.DeliveryTag, multiple: false);
            };
            channel.BasicConsume(queue: "Inventory_queue", autoAck: false, consumer: consumer);
        }

        private static async Task HandleQueueTask(string action, string bodyInString)
        {
            switch (action)
            {
                case "CREATE":
                    CreateProductDTO newProduct = JsonSerializer.Deserialize<CreateProductDTO>(
                        bodyInString
                    );
                    await _dbContext.Products.AddAsync(
                        new Product
                        {
                            Name = newProduct.Name,
                            Description = newProduct.Description,
                            Price = newProduct.Price,
                            Postedby = newProduct.PostedBy
                        }
                    );
                    await _dbContext.SaveChangesAsync();
                    break;
                case "UPDATE":
                    UpdateProductDTO updateProduct = JsonSerializer.Deserialize<UpdateProductDTO>(
                        bodyInString
                    );
                    Product productToUpdate = await _dbContext.Products.FindAsync(updateProduct.Id);
                    if (
                        productToUpdate != null
                        && productToUpdate.Postedby == updateProduct.PostedBy
                    ) //Wanted to make sure the user updating the product is the one who posted it
                    {
                        _dbContext
                            .Products.Entry(productToUpdate)
                            .CurrentValues.SetValues(updateProduct);
                    }
                    await _dbContext.SaveChangesAsync();
                    break;
                case "DELETE":
                    DeleteProductDTO deleteProduct = JsonSerializer.Deserialize<DeleteProductDTO>(
                        bodyInString
                    );
                    Product productToDelete = await _dbContext.Products.FindAsync(deleteProduct.Id);
                    if (
                        productToDelete != null
                        && productToDelete.Postedby == deleteProduct.PostedBy
                    )
                    {
                        _dbContext.Products.Remove(productToDelete);
                    }
                    await _dbContext.SaveChangesAsync();
                    break;
            }
        }
    }
}
