using Play.Common.MongoDB;
using Play.Inventory.Service.Clients;
using Play.Inventory.Service.Entities;
using Polly;
using Polly.Timeout;

var builder = WebApplication.CreateBuilder(args);

Random jitterer = new Random();
// Add services to the container.
builder.Services.AddMongo().AddMongoRepository<InventoryItem>("inventoryItems");
builder.Services.AddHttpClient<CatalogClient>(client => {
    client.BaseAddress = new Uri("http://192.168.233.129:5004");
}).AddTransientHttpErrorPolicy(policy => policy.Or<TimeoutRejectedException>().WaitAndRetryAsync(
        5, 
        retryAttempt  => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt))
                        +TimeSpan.FromMilliseconds(jitterer.Next(0, 1000)),

        onRetry: (outcome, timespan, retryAttempt) => 
        {
            var serviceProviders = builder.Services.BuildServiceProvider();
            serviceProviders.GetService<ILogger<CatalogClient>>()?
                .LogWarning($"Delaying for {timespan.TotalSeconds} seconds, then making retry {retryAttempt}");
        }
    )).AddTransientHttpErrorPolicy(policy => policy.Or<TimeoutRejectedException>().CircuitBreakerAsync(
        3, 
        TimeSpan.FromSeconds(15),
        onBreak: (outcomeType, timespan) => {
            var serviceProviders = builder.Services.BuildServiceProvider();
            serviceProviders.GetService<ILogger<CatalogClient>>()?
                .LogWarning($"Opening the circuit for  {timespan.TotalSeconds} seconds ...");
        },
        onReset: () => {
            var serviceProviders = builder.Services.BuildServiceProvider();
            serviceProviders.GetService<ILogger<CatalogClient>>()?
                .LogWarning($"Closing the circuit ...");
        }
    )).AddPolicyHandler(Policy.TimeoutAsync<HttpResponseMessage>(1));
builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
