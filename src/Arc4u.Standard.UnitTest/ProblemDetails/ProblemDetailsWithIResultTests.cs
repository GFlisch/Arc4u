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
    [Trait("Category", "CI")]
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

    #endregion
}

#endif
