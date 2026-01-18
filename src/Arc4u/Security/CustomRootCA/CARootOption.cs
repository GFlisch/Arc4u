namespace Arc4u.Security.CustomRootCA;

public sealed class CARootOption
{
    public string? CaPem { get; set; }

    public string? CaFilePath { get; set; }

    public CertificateInfo? Store { get; set; }
};
