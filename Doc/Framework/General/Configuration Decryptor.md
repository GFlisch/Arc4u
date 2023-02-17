# Arc4u.Standard.Configuration.Decryptor

#### Coming with the version 6.0.11.2 and greater.

This package is intended to be used to secure sensitive data stored in configuration. The data stored can be encrypted via a X509 certificate or via Rijndael.

Currently the .NET framework provides a way to store configuration in different sources: ini file, json or to inject them via arguments or even command line.

When it comes with sensitive data they are saying that no such kind of data must be persisted in a code repository like Github. The .NET framework is providing a non easy solution to store the sensitive data for a developer (User Secret). This solution is not a good one mainly because the secret are stored on the local pc of the developers and the information is not persisted in the repository.

It means that when a new developer is joining the team nothing is simple (just git clone and ready to start).

The best solution for me is to have the capability in the different providers to persist a data encrypted and to decrypt this when the data is read from the source. 

To achieve this the Arc4u framework is implemeted to provider:
- One based on a certificate.
- One based on Rijndael.

## How to the providers are working.

The same concept can be applied independently if we use a certificate or a Rijndael provider.

Configuration in .NET is a collection of key/value.

The Arc4u provider is fetching the list of providers defined (before it) and it will build a temprary configuration and will check all the value starting with a prefix. The default one is **Encrypt:**, but it is possble to change this.

So when a value start with "Encrypt:xyz...", the provider will extract the cypher text xyz..., decrypt it and create a new collection of key/value with the decrypted values.

As the configuration system in .NET is applying the final configuration by replacing each values based on the order of the providers. So the decrypted text will be the lates one available

## How to use the providers.

First add the package reference.

```xml
 <PackageReference Include="Arc4u.Standard.Configuration.Decryptor" Version="6.0.11.2" />
```

At the level of the program.cs, we have to add the provider we want.

```csharp

            builder.Host
                .ConfigureAppConfiguration((hostingContext, config) =>
                {
                    config.AddCertificateDecryptorConfiguration();
                });

```

### Now what is the certificate to use?

Different possibilities exist:
1) Define the information in the appsettings.{env}.json file (if you have a certificate by environment which I strongly advice).
   1) To find the certificate in the store.
   2) Via the pem files (public and private key). **Only from Arc4u 6.0.14.1 and above.**
2) Create the certificate in code and pass it as a parameter of the AddCertificateDecryptorConfiguration extension method.

> The structure of the json configuration file has changed from 6.0.14.1 to take into account the pem files certificate.

> 6.0.11.2 to 6.0.13.1
```json
{
    "EncryptionCertificate": {
    "Name": "encryptorSha2dev"
  }
}
```

> from 6.0.14.1 
```json
{
    "EncryptionCertificate": {
      "CertificateStore": {
        "Name": "encryptorSha2dev"
      },
      "File":{
        "Cert": "certs/cert.pem",
        "Key": "certs/key.pem"
      }
    }
}
```

The [CertificateInfo](https://github.com/GFlisch/Arc4u/blob/master/src/Arc4u.Standard/Security/CertificateInfo.cs) class is used to Deserialize the information from the configuration file to retrieve it.

The minimal information needed is the Name used to retrieve the certificate.

```json
{
    "EncryptionCertificate": {
    "Name": "encryptorSha2dev"
  }
}
```

The CertificateInfo class define some defaults. The search of the certificate is based on the X509FindType.FindBySubjectName on the LocalMachine certificate store in the Personal folder (My).

You can change this by specifying the value you want...

In this example, the user certificate will be used!
```json
{
    "EncryptionCertificate": {
    "Name": "encryptorSha2dev",
    "Location": "CurrentUser"
  }
}
```

The default key used to retrieve the certificate info is defined by the  key "EncryptionCertificate". The extension methods allow you to specify another key.

The same concept is applied for Rijndael, but the CertificateInfo is replaced by [RijndaelConfig](https://github.com/GFlisch/Arc4u/blob/master/src/Arc4u.Standard/Security/RijndaelConfig.cs).

