# Arc4u.OAuth2.AspNetCore

Core framework to integrate OAuth2 and creation of the principal in asp net core.

As it is a AspNetCore library, it is based on the `Microsoft.AspNetCore.Authentication` and `Microsoft.AspNetCore.Authentication.OAuth` libraries.

Do not use this package in other contexts than AspNetCore.

## ProblemDetails extension

When working with the Results feature in Arc4u, the Rest Api controller will return a `ProblemDetails` object when an error occurs.

The idea is from a controller endpoint to call the business layer and return the result to the client.
The business layer will return a `Result` object that can be converted to an `ActionResult` object.

```csharp
    return await bl.DoSomethingAsync()
                   .ToActionOkResultAsync();
```

Different situations, different HttpStatusCodes can be returned.

### Success path:  
When the call to the business layer is a success, depending on the result, different codes will be returned.

#### Result:
The return code is 204 NoContent.

#### Result\<T\>
The return code is 200 Ok or 201 Created.

##### 200 Ok HTTP GET
What to return when the value is null?  
On internet, it is not clear what to return when the value is null. Some say to return a 204 NoContent, others say to return a 200 OK with a null value.  
I find personaly than returning a 204 NoContent is more appropriate. The client knows that the call was successful but there is no content to return.  

If you are using proxy code generator like NSwagStudio, this situation is not managed like I would expect.
If you return a 200 Ok when you have a value and a 204 NoContent when the value is null, the proxy code generator will return the value when there is a value but will manage the 204 NoContent as an error!
To avoid this today, I return a 200 Ok with a null value. The client will have to manage the null value.

It is possible to override the default behavior by informing the extension method to return another code when the value is null.

```csharp
    return await bl.DoSomethingAsync(id)
                   .ToActionResultAsync(() => StatusCode(StatusCodes.Status204NoContent));
```

Mapping to a Dto is also possible. The mapper is a function and can be AutoMapper or any other mapper.

```csharp
    return await bl.DoSomethingAsync(id)
                   .ToActionResultAsync(mapper.Map<ContractDto>);
```

##### 201 Created HTTP POST
When the result is a 201 Created, the location header is set to the location of the created resource.

To handle this specific situation, the extension method `ToActionCreatedResultAsync` is available. The method expect a Uri to set the location correctly.  
The need to split the call in two steps is to allow the business layer to return the result and the controller to set the location header.  
If you do this in one step, the location is captured when the fluent Api is built and the location will never be updated when the ToActionCreatedResultAction is called!

```csharp

        Uri? location = null;

        var result = await bl.DoSomethingAsync(dto, cancellation)
                                         .OnSuccess((contract) => location = new Uri(Url.ActionLink("GetById", "Contract", new { id = contract.Id })!))
                                         .ConfigureAwait(false);

        return await result.ToActionCreatedResultAsync(location, mapper.Map<ContractDto>)
                           .ConfigureAwait(false);
```

### Error path:

When an error occurs, the business layer will return a `Result` object with the error information.

Different cases could happen:
- An exception was thrown.
- A validation  error occured.
- A Problem occured.

#### Exception
When an exception is thrown, a 500 InternalServerError is returned with a ProblemDetails message explaining that the exception is log with an activyId.
There is no more explanation about the exception to avoid leaking information.

#### Validation error
When some parameters or Dto is given to a controller, code will validate the inputs.  
If the validation fails, a 422 UnprocessableEntity is returned with a ValidationProblemDetails message explaining the validation errors.  
The code of the validation error is used to categorrized the errors (they are grouped by code).

#### Problem
When a problemDetailError is returned, a 500 InternalServerError is returned with a ProblemDetails message explaining the error when no specific StatusCode is given.

