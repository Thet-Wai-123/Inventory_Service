using System.Text;
using System.Text.Json;
using Inventory_Service.Database_Operations;
using Inventory_Service.Models;
using Marketplace_API_Gateway.DTOs;
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

                var data = JsonSerializer.Deserialize<QueueMessage>(message);
                await HandleQueueTask(data);

                channel.BasicAck(deliveryTag: ea.DeliveryTag, multiple: false);
            };
            channel.BasicConsume(queue: "Inventory_queue", autoAck: false, consumer: consumer);
        }

        private class QueueMessage
        {
            public string Action { get; set; }
            public CreateProductDTO Info { get; set; }
        }

        private static async Task HandleQueueTask(QueueMessage message)
        {
            var action = message.Action;
            var info = message.Info;
            switch (action)
            {
                case "CREATE":
                    //var product = JsonSerializer.Deserialize<CreateProductDTO>(info.ToString());
                    await _dbContext.AddAsync(
                        new Product
                        {
                            Name = info.Name,
                            Description = info.Description,
                            Price = info.Price,
                            Postedby = info.PostedBy
                        }
                    );
                    await _dbContext.SaveChangesAsync();
                    break;
                //case "UPDATE":
                //    await UpdateProduct(object.Id);
                //    break;
                //case "DELETE":
                //    await DeleteProduct(object.Id);
                //    break;
            }
        }
    }
}
