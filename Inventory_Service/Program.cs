using Inventory_Service;
using Inventory_Service.Endpoints;
using Inventory_Service.Models;
using Inventory_Service.WorkerService;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Query;

DotNetEnv.Env.TraversePath().Load();

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddDbContext<InventoryDbContext>();
builder.Services.AddSingleton<MessagingQueues>();

var app = builder.Build();

MessagingQueues.StartConsumingQueue(builder.Services);
app.MapInventoryEndpoints();

app.Run();

public partial class Program
{
}
