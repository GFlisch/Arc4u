using Arc4u.AspNetCore.Results;
using Arc4u.Results;
using Arc4u.Results.Validation;
using AutoFixture;
using AutoFixture.AutoMoq;
using FluentAssertions;
using FluentResults;
using FluentValidation;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace Arc4u.UnitTest.ProblemDetail;

sealed class ValidatorExample : AbstractValidator<string>
{
    public ValidatorExample()
    {
        RuleFor(s => s)
            .NotEmpty()
            .MaximumLength(10).WithErrorCode("100").WithSeverity(Severity.Error).WithName("Name").WithMessage("Problem");
    }
}

public class ProblemDetailsWithActionResultTests
{
    public ProblemDetailsWithActionResultTests()
    {
        _fixture = new Fixture();
        _fixture.Customize(new AutoMoqCustomization());
    }

    readonly Fixture _fixture;

    #region ValueTask

    [Fact]
    [Trait("Category", "CI")]
    public async Task Test_ValueTask_Result_To_OnSuccess_Created_With_Mapping_Should()
    {
        // arrange
        var value = Guid.NewGuid().ToString();
        var uri = _fixture.Create<Uri>();

        var result = Result.Ok(value);

        Func<ValueTask<Result<string>>> valueTask = () => ValueTask.FromResult(result);

        // act
        var sut = await valueTask().ToActionCreatedResultAsync(uri, (v) => $"{v} Arc4u");

        // assert
        sut.Value.Should().BeNull();
        sut.Result.Should().BeOfType<CreatedResult>();
        var createdResult = (CreatedResult)sut.Result!;
        createdResult!.Value.Should().Be($"{value} Arc4u");
        createdResult.Location.Should().Be(uri.ToString());
        createdResult.StatusCode.Should().Be(StatusCodes.Status201Created);
    }

    [Fact]
    [Trait("Category", "CI")]
    public async Task Test_ValueTask_Result_To_OnSuccess_Created_With_Uri_Dynamic_With_Mapping_Should()
    {
        // arrange
        var value = Guid.NewGuid().ToString();
        var uri = new Uri("about:blank");
        var okUri = _fixture.Create<Uri>();

        var result = Result.Ok(value);

        Func<ValueTask<Result<string>>> valueTask = () => ValueTask.FromResult(result);

        // act
        var sut = await valueTask()
                        .OnSuccessNotNull((v) => uri = okUri)
                        .ToActionCreatedResultAsync(uri, (v) => $"{v} Arc4u");

        // assert
        sut.Value.Should().BeNull();
        sut.Result.Should().BeOfType<CreatedResult>();
        var createdResult = (CreatedResult)sut.Result!;
        createdResult!.Value.Should().Be($"{value} Arc4u");
        createdResult.Location.Should().Be(okUri.ToString());
        createdResult.StatusCode.Should().Be(StatusCodes.Status201Created);
    }

    [Fact]
    [Trait("Category", "CI")]
    public async Task Test_ValueTask_Result_To_OnSuccess_Created_With_No_Location_With_Mapping_Should()
    {
        // arrange
        var value = Guid.NewGuid().ToString();
        Uri? uri = null;

        var result = Result.Ok(value);

        Func<ValueTask<Result<string>>> valueTask = () => ValueTask.FromResult(result);

        // act
        var sut = await valueTask().ToActionCreatedResultAsync(uri, (v) => $"{v} Arc4u");

        // assert
        sut.Value.Should().BeNull();
        sut.Result.Should().BeOfType<CreatedResult>();
        var createdResult = (CreatedResult)sut.Result!;

        createdResult!.Value.Should().Be($"{value} Arc4u");
        createdResult.StatusCode.Should().Be(StatusCodes.Status201Created);
    }

    [Fact]
    [Trait("Category", "CI")]
    public async Task Test_ValueTask_Result_To_OnSuccess_With_Mapping_Should()
    {
        // arrange
        var value = Guid.NewGuid().ToString();

        var result = Result.Ok(value);

        Func<ValueTask<Result<string>>> valueTask = () => ValueTask.FromResult(result);

        // act
        var sut = await valueTask().ToActionOkResultAsync((v) => $"{v} Arc4u");

        // assert
        sut.Value.Should().BeNull();
        sut.Result.Should().BeOfType<OkObjectResult>();
        var actionResult = sut.Result as OkObjectResult;
        actionResult.Should().NotBeNull();
        actionResult!.Value.Should().Be($"{value} Arc4u");

    }

    [Fact]
    [Trait("Category", "CI")]
    public async Task Test_ValueTask_Result_To_OnSuccess_With_Null_Should()
    {
        // arrange
        var value = Guid.NewGuid().ToString();

        var result = Result.Ok<string?>(null);

        Func<ValueTask<Result<string?>>> valueTask = () => ValueTask.FromResult(result);

        // act
        var sut = await valueTask().ToActionOkResultAsync((v) => $"{v} Arc4u");

        // assert
        sut.Value.Should().BeNull();
        sut.Result.Should().BeOfType<OkObjectResult>();
        var actionResult = sut.Result as OkObjectResult;
        actionResult.Should().NotBeNull();
        actionResult!.Value.Should().BeNull();

    }

    [Fact]
    [Trait("Category", "CI")]
    public async Task Test_ValueTask_Result_To_OnFailed_Should()
    {
        // arrange
        var value = Guid.NewGuid().ToString();

        var result = Result.Fail<string>(value);

        Func<ValueTask<Result<string>>> valueTask = () => ValueTask.FromResult(result);

        // act
        var sut = await valueTask().ToActionOkResultAsync((v) => $"{v} Arc4u");

        // assert
        sut.Value.Should().BeNull();
        sut.Result.Should().BeOfType<ObjectResult>();
        ((ObjectResult)sut.Result!).Value.Should().BeOfType<ProblemDetails>();
        var problem = (ProblemDetails)((ObjectResult)sut.Result!).Value!;
        problem.Should().NotBeNull();
        problem!.Title.Should().Be("Error.");
        problem.Detail.Should().Be(value);
        problem.Status.Should().Be(StatusCodes.Status500InternalServerError);
    }

    [Fact]
    [Trait("Category", "CI")]
    public async Task Test_ValueTask_Result_To_OnFailed_With_2_Errors_Should()
    {
        // arrange
        var msg1 = _fixture.Create<string>();
        var msg2 = _fixture.Create<string>();

        var error = new Error(msg2).WithMetadata("Code", "100");
        var result = Result.Fail<string>(msg1).WithError(error);

        Func<ValueTask<Result<string>>> valueTask = () => ValueTask.FromResult(result);

        // act
        var sut = await valueTask().ToActionOkResultAsync((v) => $"{v} Arc4u");

        // assert
        sut.Value.Should().BeNull();
        sut.Result.Should().BeOfType<ObjectResult>();
        ((ObjectResult)sut.Result!).Value!.Should().BeOfType<ProblemDetails>();
        var problem = (ProblemDetails)((ObjectResult)sut.Result).Value!;
        problem.Should().NotBeNull();
        problem.Should().NotBeNull();
        problem.Title.Should().Be("Error.");
        problem.Detail.Should().Be(msg1);
        problem.Status.Should().Be(StatusCodes.Status500InternalServerError);
    }

    [Fact]
    [Trait("Category", "CI")]
    public async Task Test_ValueTask_Result_To_OnSuccess_Without_Mapping_Should()
    {
        // arrange
        var value = Guid.NewGuid().ToString();

        var result = Result.Ok(value);

        Func<ValueTask<Result<string>>> valueTask = () => ValueTask.FromResult(result);

        // act
        var sut = await valueTask().ToActionOkResultAsync();

        // assert
        sut.Value.Should().BeNull();
        sut.Result.Should().BeOfType<OkObjectResult>();
        ((OkObjectResult)sut.Result!).Value.Should().Be(value);

    }

    [Fact]
    [Trait("Category", "CI")]
    public async Task Test_ValueTask_Result_To_OnFailed_Without_Mapping_Should()
    {
        // arrange
        var value = Guid.NewGuid().ToString();

        var result = Result.Fail<string>(value);

        Func<ValueTask<Result<string>>> valueTask = () => ValueTask.FromResult(result);

        // act
        var sut = await valueTask().ToActionOkResultAsync();

        // assert
        sut.Value.Should().BeNull();
        sut.Result.Should().BeOfType<ObjectResult>();
        ((ObjectResult)sut.Result!).Value.Should().BeOfType<ProblemDetails>();
        var problem = (ProblemDetails)((ObjectResult)sut.Result).Value!;
        problem.Should().NotBeNull();
        problem.Title.Should().Be("Error.");
        problem.Detail.Should().Be(value);
        problem.Status.Should().Be(StatusCodes.Status500InternalServerError);
    }

    #endregion

    #region Task<Result>

    [Fact]
    [Trait("Category", "CI")]
    public async Task Test_Task_of_Result_To_Fail_Sould()
    {
        // arrange
        var value = Guid.NewGuid().ToString();

        var result = Result.Fail(value);

        Func<Task<Result>> task = () => Task.FromResult(result);

        // act
        var sut = await task().ToActionOkResultAsync();

        // assert
        sut.Should().NotBeNull();
        sut.Should().BeOfType<ObjectResult>();
        ((ObjectResult)sut).Value.Should().BeOfType<ProblemDetails>();
        var problem = (ProblemDetails)((ObjectResult)sut).Value!;
        problem.Should().NotBeNull();
        problem.Title.Should().Be("Error.");
        problem.Detail.Should().Be(value);
        problem.Status.Should().Be(StatusCodes.Status500InternalServerError);
    }

    [Fact]
    [Trait("Category", "CI")]
    public async Task Test_Task_of_Result_To_Success_Sould()
    {
        // arrange
        var result = Result.Ok();

        Func<Task<Result>> task = () => Task.FromResult(result);

        // act
        var sut = await task().ToActionOkResultAsync();

        // assert
        sut.Should().NotBeNull();
        sut.Should().BeOfType<NoContentResult>();
    }

    #endregion

    #region Result<TResult>

    [Fact]
    [Trait("Category", "CI")]
    public void Test_Result_To_OnSuccess_Created_With_Mapping_Should()
    {
        // arrange
        var value = Guid.NewGuid().ToString();
        var uri = new Uri("about:blank");
        var okUri = _fixture.Create<Uri>();

        var result = Result.Ok<string>(value);

        // act
        var sut = result
                    .OnSuccess(() => uri = okUri)
                    .ToActionCreatedResult(uri, (v) => $"{v} Arc4u");

        // assert
        sut.Value.Should().BeNull();
        sut.Result.Should().BeOfType<CreatedResult>();
        var createdResult = (CreatedResult)sut.Result!;
        createdResult!.Value.Should().Be($"{value} Arc4u");
        //createdResult!.Value.Should().Be(value);
        createdResult.Location.Should().Be(okUri.ToString());
        createdResult.StatusCode.Should().Be(StatusCodes.Status201Created);
    }

    [Fact]
    [Trait("Category", "CI")]
    public void Test_Task_Result_To_OnSuccess_Created_With_Uri_Dynamic_With_Mapping_Should()
    {
        // arrange
        var value = Guid.NewGuid().ToString();
        var uri = new Uri("about:blank");
        var okUri = _fixture.Create<Uri>();

        var result = Result.Ok<string>(value);

        // act
        var sut = result
                            .OnSuccess(() => uri = okUri)
                            .ToActionCreatedResult(uri, (v) => $"{v} Arc4u");

        // assert
        sut.Value.Should().BeNull();
        sut.Result.Should().BeOfType<CreatedResult>();
        var createdResult = (CreatedResult)sut.Result!;
        createdResult!.Value.Should().Be($"{value} Arc4u");
        createdResult.Location.Should().Be(okUri.ToString());
        createdResult.StatusCode.Should().Be(StatusCodes.Status201Created);
    }

    [Fact]
    [Trait("Category", "CI")]
    public async Task Test_Result_T_To_OnSuccess_Without_Mapping_Should()
    {
        // arrange
        var value = Guid.NewGuid().ToString();

        var result = Result.Ok(value);

        Func<Task<Result<string>>> task = () => Task.FromResult(result);

        // act
        var sut = await task()
                        .ToActionOkResultAsync();

        // assert
        sut.Value.Should().BeNull();
        sut.Result.Should().BeOfType<OkObjectResult>();
        ((OkObjectResult)sut.Result!).Value.Should().Be(value);

    }

    [Fact]
    [Trait("Category", "CI")]
    public async Task Test_Result_T_To_OnFailed_Without_Mapping_Should()
    {
        // arrange
        var value = Guid.NewGuid().ToString();

        var result = Result.Fail<string>(value);

        Func<Task<Result<string>>> task = () => Task.FromResult(result);

        // act
        var sut = await task()
                        .ToActionOkResultAsync();

        // assert
        sut.Value.Should().BeNull();
        sut.Result.Should().BeOfType<ObjectResult>();
        ((ObjectResult)sut.Result!).Value.Should().BeOfType<ProblemDetails>();
        var problem = (ProblemDetails)((ObjectResult)sut.Result).Value!;
        problem.Should().NotBeNull();
        problem.Title.Should().Be("Error.");
        problem.Detail.Should().Be(value);
        problem.Status.Should().Be(StatusCodes.Status500InternalServerError);
    }

    [Fact]
    [Trait("Category", "CI")]
    public async Task Test_Result_T_To_OnSuccess_With_Mapping_Should()
    {
        // arrange
        var value = Guid.NewGuid().ToString();

        var result = Result.Ok(value);

        Func<Task<Result<string>>> task = () => Task.FromResult(result);

        // act
        var sut = await task().ToActionOkResultAsync((v) => $"{v} Arc4u");

        // assert
        sut.Value.Should().BeNull();
        sut.Result.Should().BeOfType<OkObjectResult>();
        ((OkObjectResult)sut.Result!).Value.Should().Be($"{value} Arc4u");

    }

    [Fact]
    [Trait("Category", "CI")]
    public void Test_Result_T_To_OnFailed_With_Mapping_Should()
    {
        // arrange
        var value = Guid.NewGuid().ToString();

        var result = Result.Fail<string>(value);

        // act
        var sut = result.ToActionOkResult((v) => $"{v} Arc4u");

        // assert
        sut.Value.Should().BeNull();
        sut.Result.Should().BeOfType<ObjectResult>();
        ((ObjectResult)sut.Result!).Value.Should().BeOfType<ProblemDetails>();
        var problem = (ProblemDetails)((ObjectResult)sut.Result).Value!;
        problem.Should().NotBeNull();
        problem.Should().NotBeNull();
        problem.Title.Should().Be("Error.");
        problem.Detail.Should().Be(value);
        problem.Status.Should().Be(StatusCodes.Status500InternalServerError);
    }

    [Fact]
    [Trait("Category", "CI")]
    public async Task Test_Result_To_OnFailed_With_Validation_Not_Specific_Async_Should()
    {
        // arrange
        var value = string.Empty;
        var validation = new ValidatorExample();
        var result = await validation.ValidateWithResultAsync(value);
        // act
        var sut = result.ToActionOkResult();

        // assert
        sut.Value.Should().BeNull();
        sut.Result.Should().BeOfType<ObjectResult>();
        ((ObjectResult)sut.Result!).Value.Should().BeOfType<ValidationProblemDetails>();
        var problem = (ValidationProblemDetails)((ObjectResult)sut.Result).Value!;
        problem.Should().NotBeNull();
        problem.Title.Should().Be("Error from validation.");
        problem.Detail.Should().BeNull();
        problem.Errors.Should().HaveCount(1);
        problem.Errors.First().Key.Should().Be("NotEmptyValidator");
        problem.Errors.First().Value[0].Should().Be("Error: 'Name' must not be empty.");
        problem.Status.Should().Be(StatusCodes.Status422UnprocessableEntity);
    }

    [Fact]
    [Trait("Category", "CI")]
    public void Test_Result_To_OnFailed_With_Validation_Not_Specific_Should()
    {
        // arrange
        var value = string.Empty;
        var validation = new ValidatorExample();
        var result = validation.ValidateWithResult(value);
        // act
        var sut = result.ToActionOkResult();

        // assert
        sut.Value.Should().BeNull();
        sut.Result.Should().BeOfType<ObjectResult>();
        ((ObjectResult)sut.Result!).Value.Should().BeOfType<ValidationProblemDetails>();
        var problem = (ValidationProblemDetails)((ObjectResult)sut.Result).Value!;
        problem.Should().NotBeNull();
        problem.Title.Should().Be("Error from validation.");
        problem.Detail.Should().BeNull();
        problem.Errors.Should().HaveCount(1);
        problem.Errors.First().Key.Should().Be("NotEmptyValidator");
        problem.Errors.First().Value[0].Should().Be("Error: 'Name' must not be empty.");
        problem.Status.Should().Be(StatusCodes.Status422UnprocessableEntity);
    }

    [Fact]
    [Trait("Category", "CI")]
    public async Task Test_Result_With_A_ProblemDetails_Should()
    {
        // arrange
        var detail = _fixture.Create<string>();
        var title = _fixture.Create<string>();
        var result = Result.Fail(ProblemDetailError.Create(detail).WithTitle(title));

        // act
        var sut = await result.ToActionOkResultAsync();

        // assert
        sut.Should().NotBeNull();
        sut.Should().BeOfType<ObjectResult>();
        ((ObjectResult)sut).Value.Should().BeOfType<ProblemDetails>();
        var problem = (ProblemDetails)((ObjectResult)sut).Value!;
        problem.Should().NotBeNull();
        problem.Title.Should().Be(title);
        problem.Detail.Should().Be(detail);
        problem.Status.Should().Be(StatusCodes.Status500InternalServerError);
    }

    [Fact]
    [Trait("Category", "CI")]
    public async Task Test_Result_With_An_Exception_ProblemDetails_Should()
    {
        // arrange
        Result<string> globalResult = Result.Ok();

        Func<Task<string>> error = () => throw new DbUpdateException();

        var message = _fixture.Create<string>();
        var uri = _fixture.Create<Uri>();
        var title = _fixture.Create<string>();

        await Result.Try(() => error(), (ex) => ProblemDetailError.Create(message).WithType(uri).WithTitle(title))
                    .OnFailed<string>(globalResult);

        // act
        var sut = globalResult.ToActionOkResult();

        // assert
        sut.Should().NotBeNull();
        sut.Result.Should().BeOfType<ObjectResult>();
        ((ObjectResult)sut.Result!).Value.Should().BeOfType<ProblemDetails>();
        var problem = (ProblemDetails)((ObjectResult)sut.Result).Value!;
        problem.Detail.Should().Be(message);
        problem.Title.Should().Be(title);
        problem.Type.Should().Be(uri.ToString());
    }

    [Fact]
    [Trait("Category", "CI")]
    public async Task Test_Result_With_An_Exception_Should()
    {
        // arrange
        Result<string> globalResult = Result.Ok();

        Func<Task<string>> error = () => throw new DbUpdateException();

        await Result.Try(() => error())
                    .OnFailed<string>(globalResult);

        // act
        var sut = globalResult.ToActionOkResult();

        // assert
        sut.Should().NotBeNull();
        sut.Result.Should().BeOfType<ObjectResult>();
        ((ObjectResult)sut.Result!).Value.Should().BeOfType<ProblemDetails>();
        var problem = (ProblemDetails)((ObjectResult)sut.Result).Value!;
        problem.Type.Should().Be("about:blank");
        problem.Instance.Should().BeNull();
        problem.Title.Should().NotBeEmpty();
        problem.Detail.Should().NotBeEmpty();
    }

    #endregion

    #region Result
    [Fact]
    [Trait("Category", "CI")]
    public async Task Test_Result_To_OnSuccess_Should()
    {
        // arrange
        var result = Result.Ok();

        // act
        var sut = await result.ToActionOkResultAsync();

        // assert
        sut.Should().BeOfType<NoContentResult>();
    }

    [Fact]
    [Trait("Category", "CI")]
    public async Task Test_Result_To_OnFailed_Should()
    {
        // arrange
        var value = Guid.NewGuid().ToString();

        var result = Result.Fail(value);

        // act
        var sut = await result.ToActionOkResultAsync();

        // assert
        sut.Should().NotBeNull();
        sut.Should().BeOfType<ObjectResult>();
        ((ObjectResult)sut).Value.Should().BeOfType<ProblemDetails>();
        var problem = (ProblemDetails)((ObjectResult)sut).Value!;
        problem.Should().NotBeNull();
        problem.Title.Should().Be("Error.");
        problem.Detail.Should().Be(value);
        problem.Status.Should().Be(StatusCodes.Status500InternalServerError);
    }

    [Fact]
    [Trait("Category", "CI")]
    public void Test_Result_To_OnFailed_With_Validation_Specific_Should()
    {
        // arrange
        var value = Guid.NewGuid().ToString();
        var validation = new ValidatorExample();
        var result = validation.ValidateWithResult(value);

        // act
        var sut = result.ToActionOkResult();

        // assert
        sut.Should().NotBeNull();
        sut.Result.Should().BeOfType<ObjectResult>();
        ((ObjectResult)sut.Result!).Value.Should().BeOfType<ValidationProblemDetails>();
        var problem = (ValidationProblemDetails)((ObjectResult)sut.Result).Value!;
        problem.Should().NotBeNull();
        problem.Title.Should().Be("Error from validation.");
        problem.Detail.Should().BeNull();
        problem.Errors.Should().HaveCount(1);
        problem.Errors.First().Key.Should().Be("100");
        problem.Errors.First().Value[0].Should().Be("Error: Problem");
        problem.Status.Should().Be(StatusCodes.Status422UnprocessableEntity);

    }

    #endregion
}
