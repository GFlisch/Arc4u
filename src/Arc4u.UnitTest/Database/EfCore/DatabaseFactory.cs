using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace Arc4u.UnitTest.Database.EfCore;

public class DatabaseFactory : IDesignTimeDbContextFactory<DatabaseContext>
{
    private readonly DbContextOptions<DatabaseContext> Options;

    public DatabaseFactory(DbContextOptions<DatabaseContext> dbContextOptions)
    {
        Options = dbContextOptions;
    }

    public DatabaseContext CreateDbContext(string[] args)
    {
        return new DatabaseContext(Options);
    }
}
