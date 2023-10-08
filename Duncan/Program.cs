using Duncan;
using Duncan.Interfaces;
using Duncan.Model;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddSingleton<IMapGenerator, MapGeneratorWrapper>();
builder.Services.AddSingleton<List<User>>();
builder.Services.AddSingleton<List<UserWithUnits>>();

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
    {
        c.EnableAnnotations();
    });

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseAuthorization();

app.MapControllers();

app.Run();

namespace Shard.Duncan {
    public partial class Program
    {

    } 
}



