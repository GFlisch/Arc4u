#if NET8_0_OR_GREATER

using AutoFixture.AutoMoq;
using AutoFixture;
using Xunit;
using FluentResults;
using Microsoft.AspNetCore.Http;
using Arc4u.AspNetCore.Results;
using FluentAssertions;
using Microsoft.AspNetCore.Http.HttpResults;
using Arc4u.Results;
using Arc4u.UnitTest.ProblemDetail;
using Microsoft.AspNetCore.Mvc;
using Arc4u.Results.Validation;

namespace Arc4u.Standard.UnitTest.ProblemDetails;
public class ProblemDetailsWithTypedResultTests
{
    public ProblemDetailsWithTypedResultTests()
    {
        _fixture = new Fixture();
        _fixture.Customize(new AutoMoqCustomization());
    }

    readonly Fixture _fixture;

    #region ValueTask

    [Fact]
    public async Task Test_ValueTask_Result_To_OnSuccess_Created_With_Mapping_Should()
    {
        // arrange
        var value = Guid.NewGuid().ToString();
        var uri = _fixture.Create<Uri>();

        var result = Result.Ok(value);

        Func<ValueTask<Result<string>>> valueTask = () => ValueTask.FromResult(result);

        // act
        var sut = await valueTask()
                            .ToTypedCreatedResultAsync(uri, (v) => $"{v} Arc4u");

        // assert
        sut.Should().NotBeNull();
        sut.Should().BeOfType<Results<Created<string>, ProblemHttpResult, ValidationProblem>>();
        var createdResult = (Created<string>)sut.Result;
        createdResult!.Value.Should().Be($"{value} Arc4u");
        createdResult.Location.Should().Be(uri.ToString());
        createdResult.StatusCode.Should().Be(StatusCodes.Status201Created);
    }

    [Fact]
    public async Task Test_ValueTask_Result_To_OnSuccess_Created_With_Uri_Dynamic_With_Mapping_Should()
    {
        // arrange
        var value = Guid.NewGuid().ToString();
        var uri = new Uri("about:blank");
        var okUri = _fixture.Create<Uri>();

        var result = Result.Ok(value);

        Func<ValueTask<Result<string?>>> valueTask = () => ValueTask.FromResult(result);

        // act
        var sut = await valueTask()
                        .OnSuccessNotNull((v) => uri = okUri)
                        .ToTypedCreatedResultAsync(uri, (v) => $"{v} Arc4u");

        // assert
        sut.Should().NotBeNull();
        sut.Should().BeOfType<Results<Created<string>, ProblemHttpResult, ValidationProblem>>();
        var createdResult = (Created<string>)sut.Result;
        createdResult!.Value.Should().Be($"{value} Arc4u");
        createdResult.Location.Should().Be(okUri.ToString());
        createdResult.StatusCode.Should().Be(StatusCodes.Status201Created);
    }

    [Fact]
    public async Task Test_ValueTask_Result_To_OnSuccess_Created_With_No_Location_With_Mapping_Should()
    {
        // arrange
        var value = Guid.NewGuid().ToString();
        Uri? uri = null;

        var result = Result.Ok(value);

        Func<ValueTask<Result<string>>> valueTask = () => ValueTask.FromResult(result);

        // act
        var sut = await valueTask()
                          .ToTypedCreatedResultAsync(uri, (v) => $"{v} Arc4u");

        // assert
        sut.Should().NotBeNull();
        sut.Should().BeOfType<Results<Created<string>, ProblemHttpResult, ValidationProblem>>();
        var createdResult = (Created<string>)sut.Result;
        createdResult!.Value.Should().Be($"{value} Arc4u");
        createdResult.Location.Should().BeNull();
        createdResult.StatusCode.Should().Be(StatusCodes.Status201Created);
    }

    [Fact]
    public async Task Test_ValueTask_Result_To_OnSuccess_With_Mapping_Should()
    {
        // arrange
        var value = Guid.NewGuid().ToString();

        var result = Result.Ok(value);

        Func<ValueTask<Result<string>>> valueTask = () => ValueTask.FromResult(result);

        // act
        var sut = await valueTask()
                          .ToTypedOkResultAsync((v) => $"{v} Arc4u");

        // assert
        sut.Should().NotBeNull();
        sut.Should().BeOfType<Results<Ok<string>, ProblemHttpResult, ValidationProblem>>();
        var okResult = (Ok<string>)sut.Result;
        okResult!.Value.Should().Be($"{value} Arc4u");

    }

    [Fact]
    public async Task Test_ValueTask_Result_To_OnSuccess_With_Null_Should()
    {
        // arrange
        var value = Guid.NewGuid().ToString();

        var result = Result.Ok<string>(null);

        Func<ValueTask<Result<string>>> valueTask = () => ValueTask.FromResult(result);

        // act
        var sut = await valueTask()
                          .ToTypedOkResultAsync((v) => $"{v} Arc4u");

        // assert
        sut.Should().NotBeNull();
        sut.Should().BeOfType<Results<Ok<string>, ProblemHttpResult, ValidationProblem>>();
        sut.Result.Should().BeOfType<Ok<string>>();
    }

    [Fact]
    public async Task Test_ValueTask_Result_To_OnFailed_Should()
    {
        // arrange
        var value = Guid.NewGuid().ToString();

        var result = Result.Fail<string>(value);

        Func<ValueTask<Result<string>>> valueTask = () => ValueTask.FromResult(result);

        // act
        var sut = await valueTask()
                          .ToTypedOkResultAsync((v) => $"{v} Arc4u");

        // assert
        sut.Should().NotBeNull();
        sut.Should().BeOfType<Results<Ok<string>, ProblemHttpResult, ValidationProblem>>();
        var problem = ((ProblemHttpResult)sut.Result).ProblemDetails;
        problem.Should().NotBeNull();
        problem.Title.Should().Be("Error.");
        problem.Detail.Should().Be(value);
        problem.Status.Should().Be(StatusCodes.Status500InternalServerError);
    }

    [Fact]
    public async Task Test_ValueTask_Result_To_OnFailed_With_2_Errors_Should()
    {
        // arrange
        var msg1 = _fixture.Create<string>();
        var msg2 = _fixture.Create<string>();

        var error = new Error(msg2).WithMetadata("Code", "100");
        var result = Result.Fail<string>(msg1).WithError(error);

        Func<ValueTask<Result<string>>> valueTask = () => ValueTask.FromResult(result);

        // act
        var sut = await valueTask()
                          .ToTypedOkResultAsync((v) => $"{v} Arc4u");

        // assert
        sut.Should().NotBeNull();
        sut.Should().BeOfType<Results<Ok<string>, ProblemHttpResult, ValidationProblem>>();
        var problem = ((ProblemHttpResult)sut.Result).ProblemDetails;
        problem.Should().NotBeNull();
        problem.Should().NotBeNull();
        problem.Title.Should().Be("Error.");
        problem.Detail.Should().Be(msg1);
        problem.Status.Should().Be(StatusCodes.Status500InternalServerError);

        //problem = problems[1];
        //problem.Should().NotBeNull();
        //problem.Title.Should().Be("Error.");
        //problem.Detail.Should().Be(msg2);
        //problem.Status.Should().Be(StatusCodes.Status500InternalServerError);
        //problem.Extensions.Count.Should().Be(2);
        //problem.Extensions["Code"].Should().Be("100");
    }

    [Fact]
    public async Task Test_ValueTask_Result_To_OnFailed_With_Validation_Specific_Should()
    {
        // arrange
        var value = Guid.NewGuid().ToString();
        var validation = new ValidatorExample();
        var result = validation.ValidateWithResult(value);

        Func<ValueTask<Result<string>>> valueTask = () => ValueTask.FromResult(result);

        // act
        var sut = await valueTask().ToTypedOkResultAsync();

        // assert
        sut.Should().NotBeNull();
        sut.Should().BeOfType<Results<Ok<string>, ProblemHttpResult, ValidationProblem>>();
        var problem = ((ProblemHttpResult)sut.Result).ProblemDetails as ValidationProblemDetails;
        problem.Should().NotBeNull();
        problem!.Title.Should().Be("Error from validation.");
        problem.Detail.Should().BeNull();
        problem.Errors.Should().HaveCount(1);
        problem.Errors.First().Key.Should().Be("100");
        problem.Errors.First().Value[0].Should().Be("Error: Problem");
        problem.Status.Should().Be(StatusCodes.Status422UnprocessableEntity);

    }

    [Fact]
    public async Task Test_ValueTask_Result_To_OnSuccess_Without_Mapping_Should()
    {
        // arrange
        var value = Guid.NewGuid().ToString();

        var result = Result.Ok(value);

        Func<ValueTask<Result<string>>> valueTask = () => ValueTask.FromResult(result);

        // act
        var sut = await valueTask()
                          .ToTypedOkResultAsync();

        // assert
        sut.Should().NotBeNull();
        sut.Should().BeOfType<Results<Ok<string>, ProblemHttpResult, ValidationProblem>>();
        var okResult = (Ok<string>)sut.Result;
        okResult!.Value.Should().Be(value);
    }

    [Fact]
    public async Task Test_ValueTask_Result_To_OnFailed_Without_Mapping_Should()
    {
        // arrange
        var value = Guid.NewGuid().ToString();

        var result = Result.Fail<string>(value);

        Func<ValueTask<Result<string>>> valueTask = () => ValueTask.FromResult(result);

        // act
        var sut = await valueTask()
                          .ToTypedOkResultAsync();

        // assert
        sut.Should().NotBeNull();
        sut.Should().BeOfType<Results<Ok<string>, ProblemHttpResult, ValidationProblem>>();
        var problem = ((ProblemHttpResult)sut.Result).ProblemDetails;
        problem.Should().NotBeNull();
        problem.Should().NotBeNull();
        problem.Title.Should().Be("Error.");
        problem.Detail.Should().Be(value);
        problem.Status.Should().Be(StatusCodes.Status500InternalServerError);
    }

    [Fact]
    public async Task Test_ValueTask_of_Result_To_Success_Should()
    {
        // arrange
        var result = Result.Ok();

        Func<ValueTask<Result>> valueTask = () => ValueTask.FromResult(result);

        // act
        var sut = await valueTask()
                          .ToTypedOkResultAsync();

        // assert
        sut.Should().NotBeNull();
        sut.Should().BeOfType<Results<NoContent, ProblemHttpResult, ValidationProblem>>();
        sut.Result.Should().BeOfType<NoContent>();
    }
    #endregion

    #region Task

    [Fact]
    public async Task Test_Task_Result_To_OnSuccess_Created_With_Mapping_Should()
    {
        // arrange
        var value = Guid.NewGuid().ToString();
        var uri = _fixture.Create<Uri>();

        var result = Result.Ok(value);

        Func<ValueTask<Result<string>>> task = () => ValueTask.FromResult(result);

        // act
        var sut = await task()
                            .ToTypedCreatedResultAsync(uri, (v) => $"{v} Arc4u");

        // assert
        sut.Should().NotBeNull();
        sut.Should().BeOfType<Results<Created<string>, ProblemHttpResult, ValidationProblem>>();
        var createdResult = (Created<string>)sut.Result;
        createdResult!.Value.Should().Be($"{value} Arc4u");
        createdResult.Location.Should().Be(uri.ToString());
        createdResult.StatusCode.Should().Be(StatusCodes.Status201Created);
    }

    [Fact]
    public async Task Test_Task_Result_To_OnSuccess_Created_With_Uri_Dynamic_With_Mapping_Should()
    {
        // arrange
        var value = Guid.NewGuid().ToString();
        var uri = new Uri("about:blank");
        var okUri = _fixture.Create<Uri>();

        var result = Result.Ok(value);

        Func<ValueTask<Result<string?>>> task = () => ValueTask.FromResult(result);

        // act
        var sut = await task()
                        .OnSuccessNotNull((v) => uri = okUri)
                        .ToTypedCreatedResultAsync(uri, (v) => $"{v} Arc4u");

        // assert
        sut.Should().NotBeNull();
        sut.Should().BeOfType<Results<Created<string>, ProblemHttpResult, ValidationProblem>>();
        var createdResult = (Created<string>)sut.Result;
        createdResult!.Value.Should().Be($"{value} Arc4u");
        createdResult.Location.Should().Be(okUri.ToString());
        createdResult.StatusCode.Should().Be(StatusCodes.Status201Created);
    }

    [Fact]
    public async Task Test_Task_Result_To_OnSuccess_Created_With_No_Location_With_Mapping_Should()
    {
        // arrange
        var value = Guid.NewGuid().ToString();
        Uri? uri = null;

        var result = Result.Ok(value);

        Func<ValueTask<Result<string>>> task = () => ValueTask.FromResult(result);

        // act
        var sut = await task()
                          .ToTypedCreatedResultAsync(uri, (v) => $"{v} Arc4u");

        // assert
        sut.Should().NotBeNull();
        sut.Should().BeOfType<Results<Created<string>, ProblemHttpResult, ValidationProblem>>();
        var createdResult = (Created<string>)sut.Result;
        createdResult!.Value.Should().Be($"{value} Arc4u");
        createdResult.Location.Should().BeNull();
        createdResult.StatusCode.Should().Be(StatusCodes.Status201Created);
    }

    [Fact]
    public async Task Test_Task_Result_To_OnSuccess_With_Mapping_Should()
    {
        // arrange
        var value = Guid.NewGuid().ToString();

        var result = Result.Ok(value);

        Func<ValueTask<Result<string>>> task = () => ValueTask.FromResult(result);

        // act
        var sut = await task()
                          .ToTypedOkResultAsync((v) => $"{v} Arc4u");

        // assert
        sut.Should().NotBeNull();
        sut.Should().BeOfType<Results<Ok<string>, ProblemHttpResult, ValidationProblem>>();
        var okResult = (Ok<string>)sut.Result;
        okResult!.Value.Should().Be($"{value} Arc4u");

    }

    [Fact]
    public async Task Test_Task_Result_To_OnSuccess_With_Null_Should()
    {
        // arrange
        var value = Guid.NewGuid().ToString();

        var result = Result.Ok<string>(null);

        Func<ValueTask<Result<string>>> task = () => ValueTask.FromResult(result);

        // act
        var sut = await task()
                          .ToTypedOkResultAsync((v) => $"{v} Arc4u");

        // assert
        sut.Should().NotBeNull();
        sut.Should().BeOfType<Results<Ok<string>, ProblemHttpResult, ValidationProblem>>();
        sut.Result.Should().BeOfType<Ok<string>>();
    }

    [Fact]
    public async Task Test_Task_Result_To_OnFailed_Should()
    {
        // arrange
        var value = Guid.NewGuid().ToString();

        var result = Result.Fail<string>(value);

        Func<ValueTask<Result<string>>> task = () => ValueTask.FromResult(result);

        // act
        var sut = await task()
                          .ToTypedOkResultAsync((v) => $"{v} Arc4u");

        // assert
        sut.Should().NotBeNull();
        sut.Should().BeOfType<Results<Ok<string>, ProblemHttpResult, ValidationProblem>>();
        var problem = ((ProblemHttpResult)sut.Result).ProblemDetails;
        problem.Should().NotBeNull();
        problem.Title.Should().Be("Error.");
        problem.Detail.Should().Be(value);
        problem.Status.Should().Be(StatusCodes.Status500InternalServerError);
    }

    [Fact]
    public async Task Test_Task_Result_To_OnFailed_With_2_Errors_Should()
    {
        // arrange
        var msg1 = _fixture.Create<string>();
        var msg2 = _fixture.Create<string>();

        var error = new Error(msg2).WithMetadata("Code", "100");
        var result = Result.Fail<string>(msg1).WithError(error);

        Func<ValueTask<Result<string>>> task = () => ValueTask.FromResult(result);

        // act
        var sut = await task()
                          .ToTypedOkResultAsync((v) => $"{v} Arc4u");

        // assert
        sut.Should().NotBeNull();
        sut.Should().BeOfType<Results<Ok<string>, ProblemHttpResult, ValidationProblem>>();
        var problem = ((ProblemHttpResult)sut.Result).ProblemDetails;
        problem.Should().NotBeNull();
        problem.Should().NotBeNull();
        problem.Title.Should().Be("Error.");
        problem.Detail.Should().Be(msg1);
        problem.Status.Should().Be(StatusCodes.Status500InternalServerError);

        //problem = problems[1];
        //problem.Should().NotBeNull();
        //problem.Title.Should().Be("Error.");
        //problem.Detail.Should().Be(msg2);
        //problem.Status.Should().Be(StatusCodes.Status500InternalServerError);
        //problem.Extensions.Count.Should().Be(2);
        //problem.Extensions["Code"].Should().Be("100");
    }

    [Fact]
    public async Task Test_Task_Result_To_OnFailed_With_Validation_Specific_Should()
    {
        // arrange
        var value = Guid.NewGuid().ToString();
        var validation = new ValidatorExample();
        var result = validation.ValidateWithResult(value);

        Func<ValueTask<Result<string>>> task = () => ValueTask.FromResult(result);

        // act
        var sut = await task().ToTypedOkResultAsync();

        // assert
        sut.Should().NotBeNull();
        sut.Should().BeOfType<Results<Ok<string>, ProblemHttpResult, ValidationProblem>>();
        var problem = ((ProblemHttpResult)sut.Result).ProblemDetails as ValidationProblemDetails;
        problem.Should().NotBeNull();
        problem!.Title.Should().Be("Error from validation.");
        problem.Detail.Should().BeNull();
        problem.Errors.Should().HaveCount(1);
        problem.Errors.First().Key.Should().Be("100");
        problem.Errors.First().Value[0].Should().Be("Error: Problem");
        problem.Status.Should().Be(StatusCodes.Status422UnprocessableEntity);

    }

    [Fact]
    public async Task Test_Task_Result_To_OnSuccess_Without_Mapping_Should()
    {
        // arrange
        var value = Guid.NewGuid().ToString();

        var result = Result.Ok(value);

        Func<ValueTask<Result<string>>> task = () => ValueTask.FromResult(result);

        // act
        var sut = await task()
                          .ToTypedOkResultAsync();

        // assert
        sut.Should().NotBeNull();
        sut.Should().BeOfType<Results<Ok<string>, ProblemHttpResult, ValidationProblem>>();
        var okResult = (Ok<string>)sut.Result;
        okResult!.Value.Should().Be(value);
    }

    [Fact]
    public async Task Test_Task_Result_To_OnFailed_Without_Mapping_Should()
    {
        // arrange
        var value = Guid.NewGuid().ToString();

        var result = Result.Fail<string>(value);

        Func<ValueTask<Result<string>>> task = () => ValueTask.FromResult(result);

        // act
        var sut = await task()
                          .ToTypedOkResultAsync();

        // assert
        sut.Should().NotBeNull();
        sut.Should().BeOfType<Results<Ok<string>, ProblemHttpResult, ValidationProblem>>();
        var problem = ((ProblemHttpResult)sut.Result).ProblemDetails;
        problem.Should().NotBeNull();
        problem.Should().NotBeNull();
        problem.Title.Should().Be("Error.");
        problem.Detail.Should().Be(value);
        problem.Status.Should().Be(StatusCodes.Status500InternalServerError);
    }

    [Fact]
    public async Task Test_Task_of_Result_To_Success_Should()
    {
        // arrange
        var result = Result.Ok();

        Func<ValueTask<Result>> task = () => ValueTask.FromResult(result);

        // act
        var sut = await task()
                          .ToTypedOkResultAsync();

        // assert
        sut.Should().NotBeNull();
        sut.Should().BeOfType<Results<NoContent, ProblemHttpResult, ValidationProblem>>();
        sut.Result.Should().BeOfType<NoContent>();
    }
    #endregion

    #region Result

    [Fact]
    public void Test_Result_To_OnSuccess_Created_With_Mapping_Should()
    {
        // arrange
        var value = Guid.NewGuid().ToString();
        var uri = _fixture.Create<Uri>();

        var result = Result.Ok(value);

        // act
        var sut = result.ToTypedCreatedResult(uri, (v) => $"{v} Arc4u");

        // assert
        sut.Should().NotBeNull();
        sut.Should().BeOfType<Results<Created<string>, ProblemHttpResult, ValidationProblem>>();
        var createdResult = (Created<string>)sut.Result;
        createdResult!.Value.Should().Be($"{value} Arc4u");
        createdResult.Location.Should().Be(uri.ToString());
        createdResult.StatusCode.Should().Be(StatusCodes.Status201Created);
    }

    [Fact]
    public void Test_Result_To_OnSuccess_Created_With_Uri_Dynamic_With_Mapping_Should()
    {
        // arrange
        var value = Guid.NewGuid().ToString();
        var uri = new Uri("about:blank");
        var okUri = _fixture.Create<Uri>();

        var result = Result.Ok(value);

        // act
        var sut = result
                        .OnSuccessNotNull((v) => uri = okUri)
                        .ToTypedCreatedResult(uri, (v) => $"{v} Arc4u");

        // assert
        sut.Should().NotBeNull();
        sut.Should().BeOfType<Results<Created<string>, ProblemHttpResult, ValidationProblem>>();
        var createdResult = (Created<string>)sut.Result;
        createdResult!.Value.Should().Be($"{value} Arc4u");
        createdResult.Location.Should().Be(okUri.ToString());
        createdResult.StatusCode.Should().Be(StatusCodes.Status201Created);
    }

    [Fact]
    public void Test_Result_To_OnSuccess_Created_With_No_Location_With_Mapping_Should()
    {
        // arrange
        var value = Guid.NewGuid().ToString();
        Uri? uri = null;

        var result = Result.Ok(value);

        // act
        var sut = result
                          .ToTypedCreatedResult(uri, (v) => $"{v} Arc4u");

        // assert
        sut.Should().NotBeNull();
        sut.Should().BeOfType<Results<Created<string>, ProblemHttpResult, ValidationProblem>>();
        var createdResult = (Created<string>)sut.Result;
        createdResult!.Value.Should().Be($"{value} Arc4u");
        createdResult.Location.Should().BeNull();
        createdResult.StatusCode.Should().Be(StatusCodes.Status201Created);
    }

    [Fact]
    public void Test_Result_To_OnSuccess_With_Mapping_Should()
    {
        // arrange
        var value = Guid.NewGuid().ToString();

        var result = Result.Ok(value);

        // act
        var sut = result.ToTypedOkResult((v) => $"{v} Arc4u");

        // assert
        sut.Should().NotBeNull();
        sut.Should().BeOfType<Results<Ok<string>, ProblemHttpResult, ValidationProblem>>();
        var okResult = (Ok<string>)sut.Result;
        okResult!.Value.Should().Be($"{value} Arc4u");

    }

    [Fact]
    public void Test_Result_To_OnSuccess_With_Null_Should()
    {
        // arrange
        var value = Guid.NewGuid().ToString();

        var result = Result.Ok<string>(null);

        // act
        var sut = result.ToTypedOkResult((v) => $"{v} Arc4u");

        // assert
        sut.Should().NotBeNull();
        sut.Should().BeOfType<Results<Ok<string>, ProblemHttpResult, ValidationProblem>>();
        sut.Result.Should().BeOfType<Ok<string>>();
    }

    [Fact]
    public void Test_Result_To_OnFailed_Should()
    {
        // arrange
        var value = Guid.NewGuid().ToString();

        var result = Result.Fail<string>(value);

        // act
        var sut = result.ToTypedOkResult((v) => $"{v} Arc4u");

        // assert
        sut.Should().NotBeNull();
        sut.Should().BeOfType<Results<Ok<string>, ProblemHttpResult, ValidationProblem>>();
        var problem = ((ProblemHttpResult)sut.Result).ProblemDetails;
        problem.Should().NotBeNull();
        problem.Title.Should().Be("Error.");
        problem.Detail.Should().Be(value);
        problem.Status.Should().Be(StatusCodes.Status500InternalServerError);
    }

    [Fact]
    public void Test_Result_To_OnFailed_With_2_Errors_Should()
    {
        // arrange
        var msg1 = _fixture.Create<string>();
        var msg2 = _fixture.Create<string>();

        var error = new Error(msg2).WithMetadata("Code", "100");
        var result = Result.Fail<string>(msg1).WithError(error);

        // act
        var sut = result.ToTypedOkResult((v) => $"{v} Arc4u");

        // assert
        sut.Should().NotBeNull();
        sut.Should().BeOfType<Results<Ok<string>, ProblemHttpResult, ValidationProblem>>();
        var problem = ((ProblemHttpResult)sut.Result).ProblemDetails;
        problem.Should().NotBeNull();
        problem.Should().NotBeNull();
        problem.Title.Should().Be("Error.");
        problem.Detail.Should().Be(msg1);
        problem.Status.Should().Be(StatusCodes.Status500InternalServerError);

        //problem = problems[1];
        //problem.Should().NotBeNull();
        //problem.Title.Should().Be("Error.");
        //problem.Detail.Should().Be(msg2);
        //problem.Status.Should().Be(StatusCodes.Status500InternalServerError);
        //problem.Extensions.Count.Should().Be(2);
        //problem.Extensions["Code"].Should().Be("100");
    }

    [Fact]
    public void Test_Result_To_OnFailed_With_Validation_Specific_Should()
    {
        // arrange
        var value = Guid.NewGuid().ToString();
        var validation = new ValidatorExample();
        var result = validation.ValidateWithResult(value);

        // act
        var sut = result.ToTypedOkResult();

        // assert
        sut.Should().NotBeNull();
        sut.Should().BeOfType<Results<Ok<string>, ProblemHttpResult, ValidationProblem>>();
        var problem = ((ProblemHttpResult)sut.Result).ProblemDetails as ValidationProblemDetails;
        problem.Should().NotBeNull();
        problem!.Title.Should().Be("Error from validation.");
        problem.Detail.Should().BeNull();
        problem.Errors.Should().HaveCount(1);
        problem.Errors.First().Key.Should().Be("100");
        problem.Errors.First().Value[0].Should().Be("Error: Problem");
        problem.Status.Should().Be(StatusCodes.Status422UnprocessableEntity);

    }

    [Fact]
    public void Test_Result_To_OnSuccess_Without_Mapping_Should()
    {
        // arrange
        var value = Guid.NewGuid().ToString();

        var result = Result.Ok(value);

        // act
        var sut = result.ToTypedOkResult();

        // assert
        sut.Should().NotBeNull();
        sut.Should().BeOfType<Results<Ok<string>, ProblemHttpResult, ValidationProblem>>();
        var okResult = (Ok<string>)sut.Result;
        okResult!.Value.Should().Be(value);
    }

    [Fact]
    public void Test_Result_To_OnFailed_Without_Mapping_Should()
    {
        // arrange
        var value = Guid.NewGuid().ToString();

        var result = Result.Fail<string>(value);

        // act
        var sut = result.ToTypedOkResult();

        // assert
        sut.Should().NotBeNull();
        sut.Should().BeOfType<Results<Ok<string>, ProblemHttpResult, ValidationProblem>>();
        var problem = ((ProblemHttpResult)sut.Result).ProblemDetails;
        problem.Should().NotBeNull();
        problem.Should().NotBeNull();
        problem.Title.Should().Be("Error.");
        problem.Detail.Should().Be(value);
        problem.Status.Should().Be(StatusCodes.Status500InternalServerError);
    }

    [Fact]
    public void Test_of_Result_To_Success_Should()
    {
        // arrange
        var result = Result.Ok();

        // act
        var sut = result.ToTypedOkResult();

        // assert
        sut.Should().NotBeNull();
        sut.Should().BeOfType<Results<NoContent, ProblemHttpResult, ValidationProblem>>();
        sut.Result.Should().BeOfType<NoContent>();
    }
    #endregion
}
#endif
