# Arc4u.Results

FluentResult with fluent validators dedicated for Arc4u.

This package extends FluentResults by introducing the concepts of ProblemDetailsError and ValidationError.
Currently, there is a strong link between FluentValidation and its ValidationFailure concept for creating a ValidationError object.  
Then, there is no direct capability to construct a ValidationError.
Each ValidationFailure is converted into a ValidationError and a Result can contain more than one!

The second concept is ProblemDetailError. This is a specific error that can be used to return a problem details object as defined in the RFC 7807.  This model is also more detail than just the standard Error object from FluentResults.

The third concept implemented is the integration of FluentResults with the Logging concept of Arc4u. FluentLogger implements the interface IResultLogger.  
This is useful when you want to log the result of an operation and the result is a Result object.


