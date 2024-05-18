# Results

## ProblemDetails

Implement extension methods for `ProblemDetails` to make it easier to create `ProblemDetails` instances.

ProblemDetails is a simple POCO object and this package adds a fluent Api to create one.  

Classicaly we have
```csharp
var problemDetails = new ProblemDetails
{
    Title = "Some title",
    Detail = "Some detail",
    Status = 400,
    Type = "Some type",
    Instance = "Instance"
}
```

When using the fluent Api we can write.

```csharp
var problemDetails = new ProblemDetails()
                    .WithTitle("Some title")
                    .WithDetail("Some detail")
                    .WithStatusCode(StatusCodes.Status500InternalServerError)
                    .WithType(new Uri("about:blank"))
                    .WithSeverity(Severity.Error.ToString());
```
The method WithType asks for a Uri to be sure that the type is referring to a url.


