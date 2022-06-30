using Play.Catalog.Service.Entities;
using Play.Common.MongoDB;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddMongo().AddMongoRepository<Item>("items");

builder.Services.AddControllers(o =>
{
    o.SuppressAsyncSuffixInActionNames = false;
});
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
