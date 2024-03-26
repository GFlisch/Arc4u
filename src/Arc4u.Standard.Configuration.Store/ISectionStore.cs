namespace Arc4u.Configuration.Store;

/// <summary>
/// A contract to (re)read or construct the collection of <see cref="SectionEntity"/>s from persistent storage
/// </summary>
public interface ISectionStore
{
    /// <summary>
    /// Get the contents of a specific section
    /// </summary>
    /// <param name="key"></param>
    /// <param name="cancellationToken"></param>
    /// <returns>the specific <see cref="SectionEntity"/> or null if there is no such section.</returns>
    Task<SectionEntity?> GetAsync(string key, CancellationToken cancellationToken);


    /// <summary>
    /// Get all sections in the database for a specific version
    /// </summary>
    /// <returns>The list of all section entities. Modifying this list has no effect on the store</returns>
    List<SectionEntity> GetAll();

    /// <summary>
    /// Update one or more section entities in the store
    /// </summary>
    /// <param name="entities"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    Task UpdateAsync(IEnumerable<SectionEntity> entities, CancellationToken cancellationToken);

    /// <summary>
    /// Reset the store (i.e. remove all the <see cref="SectionEntity"/> records, causing the app to revert to the initial data
    /// </summary>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    Task ResetAsync(CancellationToken cancellationToken);

    #region Internal use

    /// <summary>
    /// This is called to add the initial set of section entities. It is not intended to be called from user code.
    /// </summary>
    /// <param name="entities"></param>
    void Add(IEnumerable<SectionEntity> entities);

    #endregion
}
