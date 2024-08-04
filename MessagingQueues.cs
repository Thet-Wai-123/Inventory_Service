using System.Text;
using System.Text.Json;
using Inventory_Service.DTOs;
using Inventory_Service.Models;
using Microsoft.EntityFrameworkCore;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace Inventory_Service.WorkerService
{
    public class MessagingQueues
    {
        private static IServiceCollection _services;

        public static void StartConsumingQueue(IServiceCollection services)
        {
            _services = services;
            var factory = new ConnectionFactory
            {
                HostName = Environment.GetEnvironmentVariable("MessagingQueueHostName"),
                UserName = Environment.GetEnvironmentVariable("MessagingQueueUserName"),
                Password = Environment.GetEnvironmentVariable("MessagingQueuePassword")
            };
            var connection = factory.CreateConnection();
            var channel = connection.CreateModel();
            var consumer = new EventingBasicConsumer(channel);
            consumer.Received += async (model, ea) =>
            {
                byte[] body = ea.Body.ToArray();
                var message = Encoding.UTF8.GetString(body);

                if (ea.BasicProperties.Headers.TryGetValue("Action", out var actionObj))
                {
                    string action = Encoding.UTF8.GetString(actionObj as byte[]);
                    await HandleQueueTask(
                        action,
                        message,
                        services.BuildServiceProvider().GetService<InventoryDbContext>()
                    );
                }

                channel.BasicAck(deliveryTag: ea.DeliveryTag, multiple: false);
            };
            channel.BasicConsume(queue: "Inventory_queue", autoAck: false, consumer: consumer);
        }

        public static async Task HandleQueueTask(
            string action,
            string bodyInString,
            InventoryDbContext dbContext
        )
        {
            switch (action)
            {
                case "CREATE":
                    CreateProductDTO newProduct = JsonSerializer.Deserialize<CreateProductDTO>(
                        bodyInString
                    );
                    await dbContext.Products.AddAsync(
                        new Product
                        {
                            Name = newProduct.Name,
                            Description = newProduct.Description,
                            Price = newProduct.Price,
                            Postedby = newProduct.PostedBy
                        }
                    );
                    await dbContext.SaveChangesAsync();
                    break;
                case "UPDATE":
                    UpdateProductDTO updateProduct = JsonSerializer.Deserialize<UpdateProductDTO>(
                        bodyInString
                    );
                    Product productToUpdate = await dbContext.Products.FindAsync(updateProduct.Id);
                    if (
                        productToUpdate != null
                        && productToUpdate.Postedby == updateProduct.PostedBy
                    ) //Wanted to make sure the user updating the product is the one who posted it
                    {
                        dbContext
                            .Products.Entry(productToUpdate)
                            .CurrentValues.SetValues(updateProduct);
                    }
                    await dbContext.SaveChangesAsync();
                    break;
                case "DELETE":
                    DeleteProductDTO deleteProduct = JsonSerializer.Deserialize<DeleteProductDTO>(
                        bodyInString
                    );
                    Product productToDelete = await dbContext.Products.FindAsync(deleteProduct.Id);
                    if (
                        productToDelete != null
                        && productToDelete.Postedby == deleteProduct.PostedBy
                    )
                    {
                        dbContext.Products.Remove(productToDelete);
                    }
                    await dbContext.SaveChangesAsync();
                    break;
            }
        }
    }
}
