using Arc4u.EfCore;
using Arc4u.UnitTest.Database.EfCore.Model;
using Arc4u.UnitTest.Infrastructure;
using AutoFixture;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace Arc4u.UnitTest.Database.EfCore;

[TestCaseOrderer("Arc4u.UnitTest.Infrastructure.PriorityOrderer", "Arc4u.UnitTest")]
[Trait("Category", "CI")]
public class EfCoreTests : BaseContainerFixture<EfCoreTests, EfCoreFixture>
{
    public EfCoreTests(EfCoreFixture fixture) : base(fixture)
    {
        using var container = fixture.CreateScope();
        using var db = container.Resolve<DatabaseContext>();
        db!.Database.EnsureCreated();
    }

    [Fact]
    [TestPriority(1)]
    public async Task TestTrackerInsertData()
    {
        var fixture = new Fixture();

        using var container = Fixture.CreateScope();
        using var db = container.Resolve<DatabaseContext>();
        for (var i = 0; i < 10; i++)
        {
            var contract = fixture.Create<Contract>();
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
        using var container = Fixture.CreateScope();
        using var db = container.Resolve<DatabaseContext>();
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

        using var container = Fixture.CreateScope();
        using var db = container.Resolve<DatabaseContext>();

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
