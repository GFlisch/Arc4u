namespace Arc4u.Data;

/// <summary>
/// Specifies which actions to consider from a class implementing <see cref="IPersistEntity"/>.
/// </summary>
[Flags]
public enum PersistChangeActions
{
    /// <summary>
    /// The none action.
    /// </summary>
    None = 1,
    /// <summary>
    /// The delete action.
    /// </summary>
    Delete = 2,
    /// <summary>
    /// The insert action.
    /// </summary>
    Insert = 4,
    /// <summary>
    /// The update action.
    /// </summary>
    Update = 8,
    /// <summary>
    /// All actions except the delete action.
    /// </summary>
    AllExceptDelete = 13,
    /// <summary>
    /// All actions except the none action.
    /// </summary>
    AllExceptNone = 14,
    /// <summary>
    /// All actions.
    /// </summary>
    All = 15,
}
