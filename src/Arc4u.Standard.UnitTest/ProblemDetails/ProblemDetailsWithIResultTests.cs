#if NET8_0_OR_GREATER

using Arc4u.AspNetCore.Results;
using Arc4u.Results;
using Arc4u.Results.Validation;
using Arc4u.UnitTest.ProblemDetail;
using AutoFixture;
using AutoFixture.AutoMoq;
using FluentAssertions;
using FluentResults;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Xunit;

namespace Arc4u.Standard.UnitTest.ProblemDetails;

[Trait("Category", "CI")]
public class ProblemDetailsWithIResultTests
{
    public ProblemDetailsWithIResultTests()
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
                            .ToHttpCreatedResultAsync(uri, (v) => $"{v} Arc4u");

        // assert
        sut.Should().NotBeNull();
        sut.Should().BeOfType<Created<string>>();
        var createdResult = (Created<string>)sut;
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
                        .ToHttpCreatedResultAsync(uri, (v) => $"{v} Arc4u");

        // assert
        sut.Should().NotBeNull();
        sut.Should().BeOfType<Created<string>>();
        var createdResult = (Created<string>)sut;
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
                          .ToHttpCreatedResultAsync(uri, (v) => $"{v} Arc4u");

        // assert
        sut.Should().NotBeNull();
        sut.Should().BeOfType<Created<string>>();
        var createdResult = (Created<string>)sut;
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
                          .ToHttpOkResultAsync((v) => $"{v} Arc4u");

        // assert
        sut.Should().NotBeNull();
        sut.Should().BeOfType<Ok<string>>();
        var okResult = (Ok<string>)sut;
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
                            .ToHttpOkResultAsync((v) => $"{v} Arc4u");

        // assert
        sut.Should().NotBeNull();
        sut.Should().BeOfType<Ok>();
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
                          .ToHttpOkResultAsync((v) => $"{v} Arc4u");

        // assert
        sut.Should().NotBeNull();
        sut.Should().BeOfType<ProblemHttpResult>();
        var problem = ((ProblemHttpResult)sut).ProblemDetails;
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
                          .ToHttpOkResultAsync((v) => $"{v} Arc4u");

        // assert
        sut.Should().NotBeNull();
        sut.Should().BeOfType<ProblemHttpResult>();
        var problem = ((ProblemHttpResult)sut).ProblemDetails;
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
        var sut = await valueTask().ToHttpOkResultAsync();

        // assert
        sut.Should().NotBeNull();
        sut.Should().BeOfType<ProblemHttpResult>();
        var problem = ((ProblemHttpResult)sut).ProblemDetails as ValidationProblemDetails;
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
                          .ToHttpOkResultAsync();

        // assert
        sut.Should().NotBeNull();
        sut.Should().BeOfType<Ok<string>>();
        var okResult = (Ok<string>)sut;
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
                          .ToHttpOkResultAsync();

        // assert
        sut.Should().NotBeNull();
        sut.Should().BeOfType<ProblemHttpResult>();
        var problem = ((ProblemHttpResult)sut).ProblemDetails;
        problem.Should().NotBeNull();
        problem.Should().NotBeNull();
        problem.Title.Should().Be("Error.");
        problem.Detail.Should().Be(value);
        problem.Status.Should().Be(StatusCodes.Status500InternalServerError);
    }

    [Fact]
    public async Task Test_VakueTask_of_Result_To_Success_Should()
    {
        // arrange
        var result = Result.Ok();

        Func<ValueTask<Result>> valueTask = () => ValueTask.FromResult(result);

        // act
        var sut = await valueTask()
                          .ToHttpOkResultAsync();

        // assert
        sut.Should().NotBeNull();
        sut.Should().BeOfType<NoContent>();
    }
    #endregion

    #region Task<Result>

    [Fact]
    public async Task Test_Task_Result_To_OnSuccess_Created_With_Mapping_Should()
    {
        // arrange
        var value = Guid.NewGuid().ToString();
        var uri = _fixture.Create<Uri>();

        var result = Result.Ok(value);

        Func<Task<Result<string>>> task = () => Task.FromResult(result);

        // act
        var sut = await task()
                            .ToHttpCreatedResultAsync(uri, (v) => $"{v} Arc4u");

        // assert
        sut.Should().NotBeNull();
        sut.Should().BeOfType<Created<string>>();
        var createdResult = (Created<string>)sut;
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

        Func<Task<Result<string?>>> task = () => Task.FromResult(result);

        // act
        var sut = await task()
                        .OnSuccessNotNull((v) => uri = okUri)
                        .ToHttpCreatedResultAsync(uri, (v) => $"{v} Arc4u");

        // assert
        sut.Should().NotBeNull();
        sut.Should().BeOfType<Created<string>>();
        var createdResult = (Created<string>)sut;
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

        Func<Task<Result<string>>> task = () => Task.FromResult(result);

        // act
        var sut = await task()
                          .ToHttpCreatedResultAsync(uri, (v) => $"{v} Arc4u");

        // assert
        sut.Should().NotBeNull();
        sut.Should().BeOfType<Created<string>>();
        var createdResult = (Created<string>)sut;
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

        Func<Task<Result<string>>> task = () => Task.FromResult(result);

        // act
        var sut = await task()
                          .ToHttpOkResultAsync((v) => $"{v} Arc4u");

        // assert
        sut.Should().NotBeNull();
        sut.Should().BeOfType<Ok<string>>();
        var okResult = (Ok<string>)sut;
        okResult!.Value.Should().Be($"{value} Arc4u");

    }

    [Fact]
    public async Task Test_Task_Result_To_OnSuccess_With_Null_Should()
    {
        // arrange
        var value = Guid.NewGuid().ToString();

        var result = Result.Ok<string>(null);

        Func<Task<Result<string>>> task = () => Task.FromResult(result);

        // act
        var sut = await task()
                            .ToHttpOkResultAsync((v) => $"{v} Arc4u");

        // assert
        sut.Should().NotBeNull();
        sut.Should().BeOfType<Ok>();
    }

    [Fact]
    public async Task Test_Task_Result_To_OnFailed_Should()
    {
        // arrange
        var value = Guid.NewGuid().ToString();

        var result = Result.Fail<string>(value);

        Func<Task<Result<string>>> task = () => Task.FromResult(result);

        // act
        var sut = await task()
                          .ToHttpOkResultAsync((v) => $"{v} Arc4u");

        // assert
        sut.Should().NotBeNull();
        sut.Should().BeOfType<ProblemHttpResult>();
        var problem = ((ProblemHttpResult)sut).ProblemDetails;
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

        Func<Task<Result<string>>> task = () => Task.FromResult(result);

        // act
        var sut = await task()
                          .ToHttpOkResultAsync((v) => $"{v} Arc4u");

        // assert
        sut.Should().NotBeNull();
        sut.Should().BeOfType<ProblemHttpResult>();
        var problem = ((ProblemHttpResult)sut).ProblemDetails;
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

        Func<Task<Result<string>>> task = () => Task.FromResult(result);

        // act
        var sut = await task().ToHttpOkResultAsync();

        // assert
        sut.Should().NotBeNull();
        sut.Should().BeOfType<ProblemHttpResult>();
        var problem = ((ProblemHttpResult)sut).ProblemDetails as ValidationProblemDetails;
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

        Func<Task<Result<string>>> task = () => Task.FromResult(result);

        // act
        var sut = await task()
                          .ToHttpOkResultAsync();

        // assert
        sut.Should().NotBeNull();
        sut.Should().BeOfType<Ok<string>>();
        var okResult = (Ok<string>)sut;
        okResult!.Value.Should().Be(value);
    }

    [Fact]
    public async Task Test_Task_Result_To_OnFailed_Without_Mapping_Should()
    {
        // arrange
        var value = Guid.NewGuid().ToString();

        var result = Result.Fail<string>(value);

        Func<Task<Result<string>>> task = () => Task.FromResult(result);

        // act
        var sut = await task()
                          .ToHttpOkResultAsync();

        // assert
        sut.Should().NotBeNull();
        sut.Should().BeOfType<ProblemHttpResult>();
        var problem = ((ProblemHttpResult)sut).ProblemDetails;
        problem.Should().NotBeNull();
        problem.Should().NotBeNull();
        problem.Title.Should().Be("Error.");
        problem.Detail.Should().Be(value);
        problem.Status.Should().Be(StatusCodes.Status500InternalServerError);
    }

    [Fact]
    public async Task Test_Task_of_Result_To_Success_Sould()
    {
        // arrange
        var result = Result.Ok();

        Func<Task<Result>> task = () => Task.FromResult(result);

        // act
        var sut = await task()
                          .ToHttpOkResultAsync();

        // assert
        sut.Should().NotBeNull();
        sut.Should().BeOfType<NoContent>();
    }

    #endregion

    #region Task<Result>

    [Fact]
    public void Test_Result_To_OnSuccess_Created_With_Mapping_Should()
    {
        // arrange
        var value = Guid.NewGuid().ToString();
        var uri = _fixture.Create<Uri>();

        var result = Result.Ok(value);

        // act
        var sut = result
                    .ToHttpCreatedResult(uri, (v) => $"{v} Arc4u");

        // assert
        sut.Should().NotBeNull();
        sut.Should().BeOfType<Created<string>>();
        var createdResult = (Created<string>)sut;
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
                        .ToHttpCreatedResult(uri, (v) => $"{v} Arc4u");

        // assert
        sut.Should().NotBeNull();
        sut.Should().BeOfType<Created<string>>();
        var createdResult = (Created<string>)sut;
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
                    .ToHttpCreatedResult(uri, (v) => $"{v} Arc4u");

        // assert
        sut.Should().NotBeNull();
        sut.Should().BeOfType<Created<string>>();
        var createdResult = (Created<string>)sut;
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
        var sut = result.ToHttpOkResult((v) => $"{v} Arc4u");

        // assert
        sut.Should().NotBeNull();
        sut.Should().BeOfType<Ok<string>>();
        var okResult = (Ok<string>)sut;
        okResult!.Value.Should().Be($"{value} Arc4u");

    }

    [Fact]
    public void Test_Result_To_OnSuccess_With_Null_Should()
    {
        // arrange
        var value = Guid.NewGuid().ToString();

        var result = Result.Ok<string>(null);

        // act
        var sut = result.ToHttpOkResult((v) => $"{v} Arc4u");

        // assert
        sut.Should().NotBeNull();
        sut.Should().BeOfType<Ok>();
    }

    [Fact]
    public void Test_Result_To_OnFailed_Should()
    {
        // arrange
        var value = Guid.NewGuid().ToString();

        var result = Result.Fail<string>(value);

        // act
        var sut = result.ToHttpOkResult((v) => $"{v} Arc4u");

        // assert
        sut.Should().NotBeNull();
        sut.Should().BeOfType<ProblemHttpResult>();
        var problem = ((ProblemHttpResult)sut).ProblemDetails;
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
        var sut = result.ToHttpOkResult((v) => $"{v} Arc4u");

        // assert
        sut.Should().NotBeNull();
        sut.Should().BeOfType<ProblemHttpResult>();
        var problem = ((ProblemHttpResult)sut).ProblemDetails;
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
        var sut = result.ToHttpOkResult();

        // assert
        sut.Should().NotBeNull();
        sut.Should().BeOfType<ProblemHttpResult>();
        var problem = ((ProblemHttpResult)sut).ProblemDetails as ValidationProblemDetails;
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
        var sut = result.ToHttpOkResult();

        // assert
        sut.Should().NotBeNull();
        sut.Should().BeOfType<Ok<string>>();
        var okResult = (Ok<string>)sut;
        okResult!.Value.Should().Be(value);
    }

    [Fact]
    public void Test_Result_To_OnFailed_Without_Mapping_Should()
    {
        // arrange
        var value = Guid.NewGuid().ToString();

        var result = Result.Fail<string>(value);

        // act
        var sut = result.ToHttpOkResult();

        // assert
        sut.Should().NotBeNull();
        sut.Should().BeOfType<ProblemHttpResult>();
        var problem = ((ProblemHttpResult)sut).ProblemDetails;
        problem.Should().NotBeNull();
        problem.Should().NotBeNull();
        problem.Title.Should().Be("Error.");
        problem.Detail.Should().Be(value);
        problem.Status.Should().Be(StatusCodes.Status500InternalServerError);
    }

    [Fact]
    public void Testof_Result_To_Success_Sould()
    {
        // arrange
        var result = Result.Ok();

        // act
        var sut = result.ToHttpOkResult();

        // assert
        sut.Should().NotBeNull();
        sut.Should().BeOfType<NoContent>();
    }

    #endregion
}
#endif

