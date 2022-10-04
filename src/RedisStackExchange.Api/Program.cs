using Microsoft.Extensions.Caching.Distributed;
using RedisStackExchange.Api.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddSingleton<RedisService>();

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
app.MapPost("/products/create",  (Product product, RedisService service) =>
{
    DistributedCacheEntryOptions distributedCacheEntryOptions = new DistributedCacheEntryOptions();
    distributedCacheEntryOptions.AbsoluteExpiration = DateTime.Now.AddMinutes(1);
   
    service.Add<Product>("product",product);
    return Results.Ok();
});


app.MapGet("/products", async (RedisService service) =>
{
    Product? product = service.Get<Product>("product");


    return Results.Ok(product);
}).WithName("GetProducts");
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

internal class Category
{
    public int Id { get; set; }
    public string Name { get; set; }
}