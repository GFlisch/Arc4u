# Generating a PEM certificate pair for `X509CertificateLoader`

This document describes how to generate a self-signed certificate and private key
in PEM format that are compatible with:

```csharp
X509Certificate2 FindCertificate(CertificateFilePathInfo? certificateFilePathInfo)
```

defined in `Arc4u/Security/Cryptography/X509CertificateLoader.cs`.

## What the loader expects

The method loads the pair with:

```csharp
using var ephemeral = X509Certificate2.CreateFromPemFile(certificateFilePathInfo.Cert, certificateFilePathInfo.Key);
```

This means you need **two unencrypted PEM files**:

| `CertificateFilePathInfo` property | Content                     | PEM header                       |
| ---------------------------------- | --------------------------- | -------------------------------- |
| `Cert`                             | X.509 public certificate    | `-----BEGIN CERTIFICATE-----`    |
| `Key`                              | Matching private key        | `-----BEGIN PRIVATE KEY-----`    |

> The private key **must not be password-protected** — `CreateFromPemFile`
> does not handle encrypted keys.

## Option 1 — RSA (most common)

Generate a 2048-bit RSA private key and a self-signed certificate in one command:

```bash
openssl req -x509 -newkey rsa:2048 -nodes \
  -keyout key.pem \
  -out cert.pem \
  -days 365 \
  -subj "/CN=Arc4u Test Certificate"
```

- `-nodes` writes the key **unencrypted** (required, no passphrase).
- `key.pem` → goes into `CertificateFilePathInfo.Key`
- `cert.pem` → goes into `CertificateFilePathInfo.Cert`

## Option 2 — ECDSA (smaller / faster, also supported)

```bash
openssl ecparam -name prime256v1 -genkey -noout -out key.pem
openssl req -x509 -new -key key.pem -out cert.pem -days 365 \
  -subj "/CN=Arc4u Test Certificate"
```

## Notes / gotchas

- **Private key format**: `CreateFromPemFile` reads PKCS#8
  (`-----BEGIN PRIVATE KEY-----`), as well as
  `-----BEGIN RSA PRIVATE KEY-----` / `-----BEGIN EC PRIVATE KEY-----`.
  The commands above produce PKCS#8, which is fine. To convert an old or
  encrypted key to unencrypted PKCS#8:

  ```bash
  openssl pkcs8 -topk8 -nocrypt -in old.key -out key.pem
  ```

- **Cross-platform (incl. macOS)**: the loader deliberately round-trips through
  an in-memory PKCS#12 blob (`cert.Export(X509ContentType.Pkcs12)`) to detach
  the key from the macOS keychain, so the self-signed PEM pair is all you need.

## Configuration wiring

Point your configuration section's path properties at the two files:

```json
{
  "Cert": "/path/to/cert.pem",
  "Key":  "/path/to/key.pem"
}
```

## Verifying the pair before use

Confirm the certificate and key match — both commands must print the same
public key:

```bash
openssl x509 -in cert.pem -noout -pubkey
openssl pkey -in key.pem -pubout
```

If the two public keys are identical, the pair will load cleanly through
`FindCertificate`.
