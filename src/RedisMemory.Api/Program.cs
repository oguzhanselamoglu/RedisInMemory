using Microsoft.Extensions.Caching.Distributed;
using Newtonsoft.Json;
using System;
using System.Text;

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

#region IDistributedCache operation
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

app.MapPost("/category/create", async (Category category, IDistributedCache distributedCache) =>
{
    DistributedCacheEntryOptions distributedCacheEntryOptions = new DistributedCacheEntryOptions();
    distributedCacheEntryOptions.AbsoluteExpiration = DateTime.Now.AddMinutes(1);
    string json = JsonConvert.SerializeObject(category);
    Byte[] categoryByte = Encoding.UTF8.GetBytes(json);

    await distributedCache.SetAsync("category:1", categoryByte, distributedCacheEntryOptions);
    return Results.Ok();
});

app.MapGet("/categories", async (IDistributedCache distributedCache) =>
{
    Byte[] categoryByte = await distributedCache.GetAsync("category:1");
    string categoryJson = Encoding.UTF8.GetString(categoryByte);

    var category = JsonConvert.DeserializeObject<Category>(categoryJson);
    return Results.Ok(category);
}).WithName("GetCategory");

app.MapPost("/image/cache", async (IDistributedCache distributedCache) =>
{
    DistributedCacheEntryOptions distributedCacheEntryOptions = new DistributedCacheEntryOptions();
    distributedCacheEntryOptions.AbsoluteExpiration = DateTime.Now.AddMinutes(1);
    string path = Path.Combine(Directory.GetCurrentDirectory(), "Images/download.jpg");
    byte[] imageByte = File.ReadAllBytes(path);

    await distributedCache.SetAsync("image", imageByte, distributedCacheEntryOptions);
    return Results.Ok();
});

app.MapGet("/image", async (IDistributedCache distributedCache) =>
{
    Byte[] categoryByte = await distributedCache.GetAsync("image");
    return Results.File(categoryByte, "image/jpg");
}).WithName("GetImage");

app.MapGet("/data", (IDistributedCache distributedCache) =>
{

    string d = distributedCache.GetString("data");
    return Results.Ok(d);
}).WithName("GetData");
#endregion

#region StackExcanage

#endregion

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