using Inventory_Service.Models;
using Microsoft.EntityFrameworkCore;

DotNetEnv.Env.Load();

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddDbContext<InventoryDbContext>();
var app = builder.Build();


app.MapGet("/", () => "Hello World!");

app.Run();
