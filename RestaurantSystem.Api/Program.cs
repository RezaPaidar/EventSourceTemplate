using Microsoft.EntityFrameworkCore;
using RestaurantSystem.Api.Middleware;
using RestaurantSystem.Application;
using RestaurantSystem.Infrastructure;
using RestaurantSystem.Infrastructure.Persistence.EventStore;
using RestaurantSystem.Infrastructure.ReadStore;

var builder = WebApplication.CreateBuilder(args);

// Services
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddApplication();
builder.Services.AddInfrastructure(builder.Configuration);

var app = builder.Build();

// Middleware
app.UseCustomExceptionHandling();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();

