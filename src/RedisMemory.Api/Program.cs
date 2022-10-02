using Microsoft.Extensions.Caching.Distributed;
using Newtonsoft.Json;
using System;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddStackExchangeRedisCache(options =>
{
    options.Configuration = builder.Configuration["RedisUrl"];
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

var summaries = new[]
{
    "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
};

app.MapGet("/weatherforecast", () =>
{
    var forecast = Enumerable.Range(1, 5).Select(index =>
        new WeatherForecast
        (
            DateTime.Now.AddDays(index),
            Random.Shared.Next(-20, 55),
            summaries[Random.Shared.Next(summaries.Length)]
        ))
        .ToArray();
    return forecast;
})
.WithName("GetWeatherForecast");

app.MapGet("/setdata", (IDistributedCache distributedCache) =>
{
    DistributedCacheEntryOptions distributedCacheEntryOptions = new DistributedCacheEntryOptions();
    distributedCacheEntryOptions.AbsoluteExpiration = DateTime.Now.AddMinutes(1);
    distributedCache.SetString("data", "ABC", distributedCacheEntryOptions);
    return Results.Ok();
}).WithName("setdata");

app.MapPost("/products/create", async (Product product, IDistributedCache distributedCache) =>
{
    DistributedCacheEntryOptions distributedCacheEntryOptions = new DistributedCacheEntryOptions();
    distributedCacheEntryOptions.AbsoluteExpiration = DateTime.Now.AddMinutes(1);
    string json = JsonConvert.SerializeObject(product);
    await distributedCache.SetStringAsync("product:1", json, distributedCacheEntryOptions);
    return Results.Ok();
});

app.MapGet("/products", async (IDistributedCache distributedCache) =>
{
    string d = await distributedCache.GetStringAsync("product:1");
    var product = JsonConvert.DeserializeObject<Product>(d);
    return Results.Ok(product);
}).WithName("GetProducts");

app.MapGet("/data", (IDistributedCache distributedCache) =>
{

    string d = distributedCache.GetString("data");
    return Results.Ok(d);
}).WithName("GetData");

app.Run();

internal record WeatherForecast(DateTime Date, int TemperatureC, string? Summary)
{
    public int TemperatureF => 32 + (int)(TemperatureC / 0.5556);
}

internal class Product
{
    public int Id { get; set; }
    public string Name { get; set; }    

}