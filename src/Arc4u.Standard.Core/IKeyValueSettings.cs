namespace Arc4u;

/// <summary>
/// Define the informations we need to retrive a token for an account and the service uri for calling backend service.
/// </summary>
public interface IKeyValueSettings
{
    IReadOnlyDictionary<string, string> Values { get; }
}
