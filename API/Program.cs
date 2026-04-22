using API.Data;
using API.Services;
using FluentValidation;
using FluentValidation.AspNetCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Npgsql;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddControllers();

// Add FluentValidation
builder.Services.AddFluentValidationAutoValidation();
builder.Services.AddValidatorsFromAssemblyContaining<Program>();

builder.Services.AddScoped<IAgentService, AgentService>();
builder.Services.AddScoped<IPropertyService, PropertyService>();
builder.Services.AddScoped<IDataSeedService, DataSeedService>();

// Setup EF Core with PostgreSQL
var connectionString =
    builder.Configuration["DB_CONNECTION_STRING"]
    ?? Environment.GetEnvironmentVariable("DB_CONNECTION_STRING");
var dataSourceBuild = new NpgsqlDataSourceBuilder(connectionString);

var dataSource = dataSourceBuild.Build();

builder.Services.AddDbContext<AppDbContext>(options =>
{
    options.UseNpgsql(
        dataSource,
        npgsqlOptions =>
        {
            npgsqlOptions.MigrationsAssembly(typeof(AppDbContext).Assembly.FullName);
        }
    );

    options.UseSnakeCaseNamingConvention();

    options.ConfigureWarnings(w => w.Ignore(CoreEventId.ManyServiceProvidersCreatedWarning));
});

var app = builder.Build();

// Apply migrations and seed data
using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    await context.Database.MigrateAsync();

    var seedService = scope.ServiceProvider.GetRequiredService<IDataSeedService>();
    await seedService.SeedAsync();
}

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();
app.MapControllers();

app.Run();

public partial class Program { }
