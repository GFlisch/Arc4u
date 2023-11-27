using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Arc4u.Configuration.Store.EFCore.Internals;

/// <summary>
/// An implementation of a <see cref="ISectionStore"/> by using a <see cref="DbContext"/>.
/// This is expected to be a scoped service.
/// </summary>
/// <typeparam name="TDbContext"></typeparam>
sealed class DbContextSectionStore<TDbContext> : ISectionStore
    where TDbContext : DbContext
{
    private readonly TDbContext _dbContext;
    private readonly ILogger<DbContextSectionStore<TDbContext>> _logger;

    public DbContextSectionStore(TDbContext dbContext, ILogger<DbContextSectionStore<TDbContext>> logger)
    {
        _dbContext = dbContext;
        _logger = logger;
    }

    private DbSet<SectionEntity> SectionEntitySet => _dbContext.Set<SectionEntity>();

    public void Add(IEnumerable<SectionEntity> entities)
    {
        try
        {
            SectionEntitySet.AddRange(entities);
            _dbContext.SaveChanges();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Adding the initial SectionEntities failed");
        }
    }

    public Task<SectionEntity?> GetAsync(string key, CancellationToken cancellationToken)
    {
        return SectionEntitySet.AsNoTracking().Where(section => section.Key == key).SingleOrDefaultAsync(cancellationToken);
    }

    public List<SectionEntity> GetAll()
    {
        return SectionEntitySet.AsNoTracking().ToList();
    }

    public Task UpdateAsync(IEnumerable<SectionEntity> entities, CancellationToken cancellationToken)
    {
        SectionEntitySet.UpdateRange(entities);
        return _dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task ResetAsync(CancellationToken cancellationToken)
    {
        var set = SectionEntitySet;
        set.RemoveRange(set.AsNoTracking().ToList());
        await _dbContext.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
    }
}

