May 2019.
# Guidance 2017.1.15

This new version fix a lot of small issues from version 2017.1.14-2 and runs on Visual Studio 2017 and 2019.
</br>
But also 2 new features:
- NServiceBus integration.
- VueJS.

## What about the new features?

### Messaging.

You can now add a messaging project to the backend and allow communication to a borker messages platform. Today only RabbitMQ is supported.

The servicebus in Azure will be supported too. 





---
### VueJS.

[VueJs](https://vuejs.org/) is a light and efficient javascript UI framework and it will be used until blazor.net is mature and released to build web frontends.

The guidance will create a physical folder structure with all the npm packages, config, layout and typescript code to build a basic UI.

The Arc4u framework to authenticate the user is based on adal.js via the package adal-angular. Updates of the framework package will come from npm store.

The same for the design (electron-design) wich is also a npm package.

The name of the UI will be the name of the folder below the FE folder in the solution.


---
## What are fixed?
- Hangfire Job service account creation.
- 