# Arc4u.Dependency.Tool

The Arc4u.Dependency.Tool contains 2 code generators.

## AttributeGenerator

This code generator will analyze the code and see if an ExportAttribute from Arc4u exits.
If yes, the code will check if SharedAttribute or ScopedAttribute is present.

Based on this evaluation the code generator will generate the corresponding registration in IServiceCollection.

Example:
```csharp
[Export(typeof(ITest), Scoped]
public Test : ITest
{
}
```

The Generator will generate:
```csharp
    services.AddTransient<ITest, Test>();
```

If a name is added to the export, we will have:

```csharp
[Export("key", typeof(ITest), Scoped]
public Test : ITest
{
}
```

The Generator will generate:
```csharp
    services.AddKeyedTransient<ITest, Test>("key");
```

The name of the extension method is based on the name of the Assembly.
Assembly name = xxx.Host => RegisterHostTypes.

## GenerateRegisteredTypes

Arc4u.Dependency let you add types in a json file to register them in the IServiceCollection.

The same capability is offered but by inspecting the nuget packages dll and validating that the expected type
is present, if yes the entry is generated.

Example:
```json
{
  "Application.Dependency": {
    "Assemblies": [
    ],
    "RegisterTypes": [
      "Arc4u.Caching.Memory.MemoryCache, Arc4u.Standard.Caching.Memory"
    ]
  }
}
```

The Arc4u MemoryCache is defined in the Arc4u.Standard.Caching.Memory package and contains the following ExportAttribute:

```csharp
[Export("Memory", typeof(ICache))]
public class MemoryCache : BaseDistributeCache<MemoryCache>, ICache
{
}
```

The code generator will generate the following code:

```csharp
services.AddKeyedTransient<Arc4u.Caching.ICache, Arc4u.Caching.Memory.MemoryCache>("Memory");
```

The extension method is named: RegisterTypes.

> Attention.
> The target project must add an AddtionnalFile entry including the "Configs\appsettings.json".

```xml
  <ItemGroup>
    <AdditionalFiles Include="Configs\appsettings.json" />
  </ItemGroup>
```

