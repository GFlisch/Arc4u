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
                    .WithStatusCode(StatusCodes.Status400BadRequest)
                    .WithType(new Uri("about:blank"))
                    .WithSeverity(Severity.Error.ToString());
```
The method WithType asks for a Uri to be sure that the type is referring to a url.


## From Results to ProblemDetails

As it is a common scenario to return a ProblemDetails from an action, this package provides extension methods to convert the results to ProblemDetails.

Different scenarios are covered:
- ToGenericMessage: Converts a result to a ProblemDetails with a generic message saying the information is logged.
- ToProblemDetails: Converts a result to a ProblemDetails or a generic one if the errors contain at least one exception.

The developers will generaly not used the ToGenericMessage method, but it is used by the ToProblemDetails method, the ToProblemDetails method is taking care to manage this to a generic message if any exception(s) are part of the error messages collection.

A default Function implementation exists to convert an exception to a ProblemDetails, but it is possible to provide a custom implementation.
Just use the static method 'SetFromErrorFactory(Func<IEnumerable<IError>, ProblemDetails> fromErrors)' to set your custom implementation.

The Arc4u framework covers the following Error types:
- IExceptionalError => will result in a Generic message with an Http Status Code equal to 500.
- ProblemDetailsError => will be converted to a ProblemDetails with a default Http Status Code equal to 400 if another one is not given!
- ValidationError => will be converted to a ValidationProblemDetails with a Http Status Code equal to 422.

If you have your own error type, you can implement the IError interface and provide a custom implementation to convert it to a ProblemDetails.
