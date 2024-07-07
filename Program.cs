using Inventory_Service.Database_Operations;
using Inventory_Service.Endpoints;
using Inventory_Service.Models;
using Inventory_Service.WorkerService;
using Microsoft.EntityFrameworkCore;

DotNetEnv.Env.Load();

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddDbContext<InventoryDbContext>();

var app = builder.Build();

MessagingQueues.StartConsumingQueue(
    builder.Services.BuildServiceProvider().GetService<InventoryDbContext>()
);
app.MapInventoryEndpoints();

app.Run();
