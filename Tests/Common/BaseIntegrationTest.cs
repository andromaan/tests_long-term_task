using API.Data;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace Tests.Common;

public abstract class BaseIntegrationTest : IClassFixture<IntegrationTestWebFactory>
{
    protected readonly AppDbContext Context;
    protected readonly HttpClient Client;
    protected readonly IntegrationTestWebFactory Factory;

    protected BaseIntegrationTest(IntegrationTestWebFactory factory)
    {
        Factory = factory;

        var scope = factory.Services.CreateScope();
        Context = scope.ServiceProvider.GetRequiredService<AppDbContext>();

        Client = factory.CreateClient();
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
    }
}
