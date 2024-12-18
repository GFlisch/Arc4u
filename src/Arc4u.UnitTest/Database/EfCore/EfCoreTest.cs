using Arc4u.EfCore;
using Arc4u.UnitTest.Database.EfCore.Model;
using Arc4u.UnitTest.Infrastructure;
using AutoFixture;
using AutoFixture.AutoMoq;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Xunit;

namespace Arc4u.UnitTest.Database.EfCore;

[TestCaseOrderer("Arc4u.UnitTest.Infrastructure.PriorityOrderer", "Arc4u.UnitTest")]
[Trait("Category", "CI")]
public class EfCoreTests
{
    public EfCoreTests()
    {
        var services = new ServiceCollection();
        services.AddSingleton(typeof(ILogger<>), typeof(NullLogger<>));
        services.AddSingleton<DatabaseFactory>();
        services.AddSingleton<DbContextOptions<DatabaseContext>>(_ =>
        {
            var optionsBuilder = new DbContextOptionsBuilder<DatabaseContext>();

            optionsBuilder.UseInMemoryDatabase("EfCore")
                          .UseQueryTrackingBehavior(QueryTrackingBehavior.NoTracking);

            return optionsBuilder.Options;
        });
        services.AddScoped<DatabaseContext, DatabaseContext>();

        _serviceProvider = services.BuildServiceProvider();
        _fixture = new Fixture();
        _fixture.Customize(new AutoMoqCustomization());
    }

    readonly Fixture _fixture;
    readonly IServiceProvider _serviceProvider;

    [Fact]
    [TestPriority(1)]
    public async Task TestTrackerInsertData()
    {
        var container = _serviceProvider.CreateScope().ServiceProvider;

        using var db = container.GetRequiredService<DatabaseContext>();
        db.Database.EnsureCreated();

        for (var i = 0; i < 10; i++)
        {
            var contract = _fixture.Create<Contract>();
            contract.PersistChange = Data.PersistChange.Insert;

            db!.ChangeTracker.TrackGraph(contract, ChangeGraphTracker.Tracker);

            await db.SaveChangesAsync(CancellationToken.None);

        }

        var result = await db!.Contracts.ToListAsync();

        Assert.Equal(10, result.Count);
    }

    [Fact]
    [TestPriority(2)]
    public async Task TestDelete()
    {
        var container = _serviceProvider.CreateScope().ServiceProvider;
        using var db = container.GetRequiredService<DatabaseContext>();
        var result = await db!.Contracts.ToListAsync();

        var toDelete = result.First();
        toDelete.PersistChange = Data.PersistChange.Delete;

        db.ChangeTracker.TrackGraph(toDelete, ChangeGraphTracker.Tracker);

        await db.SaveChangesAsync(CancellationToken.None);

        result = await db.Contracts.ToListAsync();

        Assert.Equal(9, result.Count);
    }

    [Fact]
    [TestPriority(3)]
    public async Task TestInsertAndUpdate()
    {
        var fixture = new Fixture();

        var container = _serviceProvider.CreateScope().ServiceProvider;
        using var db = container.GetRequiredService<DatabaseContext>();

        var contract = fixture.Create<Contract>();
        contract.PersistChange = Data.PersistChange.Insert;

        var id = contract.Id;

        db!.ChangeTracker.TrackGraph(contract, ChangeGraphTracker.Tracker);

        await db.SaveChangesAsync(CancellationToken.None);
        contract = null;
        db.ChangeTracker.Clear();

        contract = db.Contracts.First(c => c.Id == id);

        Assert.Equal(Data.PersistChange.None, contract.PersistChange);

        contract.PersistChange = Data.PersistChange.Update;

        contract.Name = "Updated";

        Assert.Equal(EntityState.Detached, db.Entry(contract).State);

        db.ChangeTracker.TrackGraph(contract, ChangeGraphTracker.Tracker);

        await db.SaveChangesAsync(CancellationToken.None);
        db.ChangeTracker.Clear();
    }
}
