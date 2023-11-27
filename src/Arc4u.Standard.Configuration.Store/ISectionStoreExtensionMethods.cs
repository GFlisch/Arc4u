namespace Arc4u.Configuration.Store;

public static class ISectionStoreExtensionMethods
{
    /// <summary>
    /// Update an entity in the store
    /// </summary>
    /// <param name="sectionStore"></param>
    /// <param name="entity"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public static Task UpdateAsync(this ISectionStore sectionStore, SectionEntity entity, CancellationToken cancellationToken) => sectionStore.UpdateAsync(new[] { entity }, cancellationToken);
}
