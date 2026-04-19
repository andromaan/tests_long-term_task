using API.Data;
using API.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Npgsql;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

builder.Services.AddControllers();

builder.Services.AddScoped<IAgentService, AgentService>();
builder.Services.AddScoped<IPropertyService, PropertyService>();

// Setup EF Core with PostgreSQL
var connectionString = builder.Configuration["DB_CONNECTION_STRING"] ?? Environment.GetEnvironmentVariable("DB_CONNECTION_STRING");
var dataSourceBuild = new NpgsqlDataSourceBuilder(connectionString);

var dataSource = dataSourceBuild.Build();

builder.Services.AddDbContext<AppDbContext>(options => {
    options.UseNpgsql(dataSource, npgsqlOptions =>
    {
        npgsqlOptions.MigrationsAssembly(typeof(AppDbContext).Assembly.FullName);
    });

    options.UseSnakeCaseNamingConvention();

    options.ConfigureWarnings(w =>
        w.Ignore(CoreEventId.ManyServiceProvidersCreatedWarning));
});


var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();

app.MapControllers();

app.Run();