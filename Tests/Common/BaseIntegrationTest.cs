using API.Data;
using Microsoft.Extensions.DependencyInjection;

namespace Tests.Common;

public abstract class BaseIntegrationTest : IClassFixture<IntegrationTestWebFactory>
{
    protected readonly IntegrationTestWebFactory Factory;
    protected readonly HttpClient Client;
    protected AppDbContext Context { get; set; }

    protected BaseIntegrationTest(IntegrationTestWebFactory factory)
    {
        Factory = factory;
        Client = factory.CreateClient();
        Context = CreateNewContext();
    }

    protected AppDbContext CreateNewContext()
    {
        var scope = Factory.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        context.ChangeTracker.Clear();
        return context;
    }

    protected async Task<int> SaveChangesAsync()
    {
        var result = await Context.SaveChangesAsync();
        Context.ChangeTracker.Clear();
        return result;
    }

    protected async Task ClearDatabaseAsync()
    {
        Context.Agents.RemoveRange(Context.Agents);
        Context.Properties.RemoveRange(Context.Properties);
        Context.Inquiries.RemoveRange(Context.Inquiries);
        await Context.SaveChangesAsync();
        Context.ChangeTracker.Clear();
    }
}
