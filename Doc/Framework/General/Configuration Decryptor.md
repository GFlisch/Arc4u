# Arc4u.Standard.Configuration.Decryptor

This package is intended to be used to secure sensitive data stored in configuration. The data stored can be encrypted via a X509 certificate or via Rijndael.

Currently the .NET framework provides a way to store configuration in different sources: ini file, json or to inject them via arguments or even command line.

When it comes with sensitive data they are saying that no such kind of data must be persisted in a code repository like Github. The .NET framework is providing a non easy solution to store the sensitive data for a developer (User Secret). This solution is not a good one mainly because the secret are stored on the local pc of the developers and the information is not persisted in the repository.

It means that when a new developer is joining the team nothing is simple (just git clone and ready to start).

The best solution for me is to have the capability in the different providers to persist a data encrypted and to decrypt this when the data is read from the source. 
