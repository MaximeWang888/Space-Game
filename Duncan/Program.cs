using Duncan;
using Duncan.Authentification;
using Duncan.Model;
using Duncan.Repositories;
using Duncan.Services;
using Duncan.Utils;
using Microsoft.AspNetCore.Authentication.OAuth;
using Shard.Shared.Core;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.D
builder.Services.AddSingleton<MapGeneratorWrapper>();
builder.Services.AddSingleton<IClock>(new SystemClock());
builder.Services.AddSingleton<UserDB>();
builder.Services.AddSingleton<UnitsRepo>();
builder.Services.AddSingleton<UsersRepo>();
builder.Services.AddSingleton<PlanetRepo>();
builder.Services.AddSingleton<SystemsRepo>();
builder.Services.AddSingleton<SystemsService>();
builder.Services.AddSingleton<UnitsService>();
builder.Services.AddSingleton<BuildingsService>();
builder.Services.AddHostedService<TimedHostedService>();
builder.Services.AddAuthentication()
    .AddScheme<AuthOptions, AuthHandler>(
        "SessionTokens",
        opts => { }
    );
builder.Services.Configure<Dictionary<string, Wormholes>>(builder.Configuration.GetSection("Wormholes"));
builder.Services.AddHttpClient();



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



