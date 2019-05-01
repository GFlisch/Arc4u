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

> Todo: Add video and explanation here.

---
### VueJS.

[VueJs](https://vuejs.org/) is a light and efficient javascript UI framework and it will be used until blazor.net is mature and released to build web frontends.

The guidance will create a physical folder structure with all the npm packages, config, layout and typescript code to build a basic UI.

The Arc4u framework to authenticate the user is based on adal.js via the package adal-angular. Updates of the framework package will come from npm store.

The same for the design (electron-design) wich is also a npm package.

The name of the UI will be the name of the folder below the FE folder in the solution.


[![Everything Is AWESOME](https://img.youtube.com/vi/f_dJSNeELgE/0.jpg)](https://youtu.be/f_dJSNeELgE "Arc4u Guidance - VueJS.")

At the level of Visual Studio, there is no project but only Solution folders and files. Visual Studio code is the tool after to develop and debug the VueJS app.

What's Generate?

```

Configs     => OAuth config files by environment.
public      => result of the build to deploy.
src         => The Vue application.

Files used to configure the edition and compilation.

.vscode
.editorconfig
.gitignore
babel.config.js
package.json
README.md
tsconfig.json
```

To start the application after the generation, execute:
- yarn install
- yarn run serve

The code below the src folder is organized like this.

```
 assets         => images.
 proxy          => NSwag typescript code generated to call the backend.
 views          => Vue pages.
 Access.ts      => Class defining the rights.
 App.vue        => Main page with the skeleton layout.
 GlobalResource.ts  => the string localized.
 main.ts        => Authenticate the user and start Vue.
 registerServiceWorker.ts
 router.ts      => Define the routes when surfing on a page.
 shims-tsx.d.ts
 shims-vue.d.ts
 store.ts
 tokenConfig.json   => the config to connect AzureAD or ADFS server.
```


---
## What are fixed?
- Hangfire Job service account creation.
- 