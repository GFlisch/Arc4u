# Releases of Arc4u packages.


## 6.0

### 6.0.14.2

### 6.0.14.1

Add the capability to read a certificate (for the Arc4u.Standard.Configuration.Decryptor) based on pem files for the public and secret keys.
This is to be used on K8s pods, where secret will contain the certificate using the pem format to decrypt the cypher texts.

### 6.0.13.1 &  6.0.12.1

Update packages to the corresponding .NET version.

### 6.0.11.2
Update packages used by Arc4u to the latest ones but the .NET one still on the 6.0.11 and 7.0.1

Introduce the new Arc4u.Standard.Configuration.Decryptor. This package ease the use of encrypted config and decrypt the content on the fly.
See the documentation [here](./Framework/General/Configuration Decryptor.md).

#### What's new.
- Add the .editorconfig file of ASPNET Core project to give code analysis on the projects.
- Add a new project Arc4u.Standard.Configuration.Decryptor.
  - Applying the .editorconfig rules.
- For the Blazor client project, fix an issue where the config settings was resolved based on OAuth and not OAuth2
- Value converted from the appSettings are insensitive to the culture.

### 6.0.11.1

This version of the framework is supporting .NET 6.0.11 and .NET 7.0.1.
Nuget packages have been updated to support those versions.

#### What's new.

A new project, Arc4u.Standard.OAuth2.AspNetCore.Authentication, is added to perform authentication based on standard OpenId Connect and OAuth2.
using the Microsoft standard packages:
- Microsoft.AspNetCore.Authentication.OpenIdConnect
- Microsoft.AspNetCore.Authentication.JwtBearer
 
 The project is supporting AzureAD, AzureB2C and Keycloak.
 ADFS is in the loop but not yet tested.

 This package will replace Msal, Adal (which is now unsupported by Microsoft).

 #### Breaking changes.

 The OAuthConfig is gone and replaced by different implementations but also decouple via interfaces. This will offer to you the capability to change the implementation if you have a special need.

The problem with the current implementation is the mix of concept implemented in OAuthConfig and the fact that it was not possible to inject a custom implementation. The new implementation solves those issues.

The aim of the OAuthConfig was to read from the configuration file, the claim to use to identify a user authenticated (Claim in ClaimsIdentity).
The class was implementing the read of the configuration but also the method to retrieve a unique identifier of a user and to compute the key used to store extra claims in a cache.

Now the different concepts are splitted.

The ITokenUserCacheConfiguration implementation is responsible to read the claims to use to identify specificly a user. This capability is introduce because each authority has its own way to identify a user.
- [ITokenUserCacheConfiguration](https://github.com/GFlisch/Arc4u/blob/release/6.0.11.1/src/Arc4u.Standard.OAuth2/Configuration/ITokenUserCacheConfiguration.cs)
- [TokenUserCacheConfiguration](https://github.com/GFlisch/Arc4u/blob/release/6.0.11.1/src/Arc4u.Standard.OAuth2/Configuration/TokenUserCacheConfiguration.cs)

The IUserObjectIdentifier implementation is dedicated to retrieve the claim value that will be used to uniquely identify an authenticated ClaimsIdentity. This is used to compute key to cache the token.
- [IUserObjectIdentifier](https://github.com/GFlisch/Arc4u/blob/release/6.0.11.1/src/Arc4u.Standard.OAuth2/Security/IUserObjectIdentifier.cs)
- [UserObjectIdentifier](https://github.com/GFlisch/Arc4u/blob/release/6.0.11.1/src/Arc4u.Standard.OAuth2/Security/UserObjectIdentifier.cs)

The [KeyGeneratorFromIdentity](https://github.com/GFlisch/Arc4u/blob/release/6.0.11.1/src/Arc4u.Standard.OAuth2/Security/Principal/KeyGeneratorFromIdentity.cs) implementation is changed to use the [IUserObjectIdentifier](https://github.com/GFlisch/Arc4u/blob/release/6.0.11.1/src/Arc4u.Standard.OAuth2/Security/IUserObjectIdentifier.cs) implementation to compute a key to store the extra claims (not into the token).

Arc4u.Standard.OAuth2.AspNetCore.Adal and Arc4u.Standard.OAuth2.AspNetCore.Msal projets are impacted by replacing the OAuthConfig usage by the injection of the interface.

### 6.0.9.1
=> packages updated to the version 6.0.9 of dotNet framework.

### 6.0.8.2
=> Fix a severe security issue regarding the ServiceAspect used to control the right of a user. See comment on 5.0.17.3

## 5.0

### 5.0.17.3
=> Fix a severe security issue regarding the ServiceAspect used to control the right of a user.
=> From dotNet 5.0 the Attributes are not loaded contextually for each call but loaded once. To have the context of the user the injection of the IApplicationContext will not work. The applicationContext will be the context of the first user calling an api!
