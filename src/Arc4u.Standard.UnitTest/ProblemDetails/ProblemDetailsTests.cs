using AutoFixture.AutoMoq;
using AutoFixture;
using System.Threading.Tasks;
using Xunit;
using FluentResults;
using Arc4u.OAuth2.AspNetCore.Extensions;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using Microsoft.AspNetCore.Http;
using System;

namespace Arc4u.UnitTest.ProblemDetail;
public class ProblemDetailsTests
{
    public ProblemDetailsTests()
    {
        _fixture = new Fixture();
        _fixture.Customize(new AutoMoqCustomization());
    }

    readonly Fixture _fixture;

    [Fact]
    [Trait("Category", "CI")]
    public async Task Test_ValuTask_Result_To_OnSuccess_With_Mapping_Should()
    {
        // arrange
        var value = Guid.NewGuid().ToString();

        var result = Result.Ok(value);

        Func<ValueTask<Result<string>>> valueTask = () => ValueTask.FromResult(result);

        // act
        var sut = await valueTask().ToActionResultAsync((v) => $"{v} Arc4u");

        // assert
        sut.Value.Should().BeNull();
        sut.Result.Should().BeOfType<OkObjectResult>();
        ((OkObjectResult)sut.Result).Value.Should().Be($"{value} Arc4u");
        
    }

    [Fact]
    [Trait("Category", "CI")]
    public async Task Test_ValuTask_Result_To_OnSuccess_Without_Mapping_Should()
    {
        // arrange
        var value = Guid.NewGuid().ToString();

        var result = Result.Ok(value);

        Func<ValueTask<Result<string>>> valueTask = () => ValueTask.FromResult(result);

        // act
        var sut = await valueTask().ToActionResultAsync();

        // assert
        sut.Value.Should().BeNull();
        sut.Result.Should().BeOfType<OkObjectResult>();
        ((OkObjectResult)sut.Result).Value.Should().Be(value);

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
        var sut = await valueTask().ToActionResultAsync((v) => $"{v} Arc4u");

        // assert
        sut.Value.Should().BeNull();
        sut.Result.Should().BeOfType<BadRequestObjectResult>();
        ((BadRequestObjectResult)sut.Result).Value.Should().BeOfType<List<ProblemDetails>>();
        var problems = (List<ProblemDetails>)((BadRequestObjectResult)sut.Result).Value;
        problems.Should().NotBeNull();
        problems.Count.Should().Be(1);
        var problem = problems[0];
        problem.Should().NotBeNull();
        problem.Title.Should().Be("Error.");
        problem.Detail.Should().Be(value);
        problem.Status.Should().Be(StatusCodes.Status400BadRequest);
    }

    [Fact]
    [Trait("Category", "CI")]
    public async Task Test_Task_of_Result_To_Fail_Sould()
    {
        // arrange
        var value = Guid.NewGuid().ToString();

        var result = Result.Fail(value);

        Func<Task<Result>> task = () => Task.FromResult(result);

        // act
        var sut = await task().ToActionResultAsync();

        // assert
        sut.Should().NotBeNull();
        sut.Should().BeOfType<BadRequestObjectResult>();

        var badRequest = (BadRequestObjectResult)sut;
        badRequest.Value.Should().BeOfType<List<ProblemDetails>>();
        var problems = (List<ProblemDetails>)badRequest.Value;
        problems.Should().NotBeNull();
        problems.Count.Should().Be(1);
        var problem = problems[0];
        problem.Should().NotBeNull();
        problem.Title.Should().Be("Error.");
        problem.Detail.Should().Be(value);
        problem.Status.Should().Be(StatusCodes.Status400BadRequest);
    }

    [Fact]
    [Trait("Category", "CI")]
    public async Task Test_Task_of_Result_To_Success_Sould()
    {
        // arrange
        var result = Result.Ok();

        Func<Task<Result>> task = () => Task.FromResult(result);

        // act
        var sut = await task().ToActionResultAsync();

        // assert
        sut.Should().NotBeNull();
        sut.Should().BeOfType<OkResult>();
    }

    [Fact]
    [Trait("Category", "CI")]
    public async Task Test_Task_Result_To_OnSuccess_Without_Mapping_Should()
    {
        // arrange
        var value = Guid.NewGuid().ToString();

        var result = Result.Ok(value);

        Func<Task<Result<string>>> task = () => Task.FromResult(result);

        // act
        var executioin = await task();
        var sut = await executioin.ToActionResultAsync();

        // assert
        sut.Value.Should().BeNull();
        sut.Result.Should().BeOfType<OkObjectResult>();
        ((OkObjectResult)sut.Result).Value.Should().Be(value);

    }

    [Fact]
    [Trait("Category", "CI")]
    public async Task Test_Task_Result_To_OnSuccess_With_Mapping_Should()
    {
        // arrange
        var value = Guid.NewGuid().ToString();

        var result = Result.Ok(value);

        Func<Task<Result<string>>> task = () => Task.FromResult(result);

        // act
        var executioin = await task();
        var sut = await executioin.ToActionResultAsync((v) => $"{v} Arc4u");

        // assert
        sut.Value.Should().BeNull();
        sut.Result.Should().BeOfType<OkObjectResult>();
        ((OkObjectResult)sut.Result).Value.Should().Be($"{value} Arc4u");

    }

    [Fact]
    [Trait("Category", "CI")]
    public async Task Test_Task_Result_To_OnFailed_Should()
    {
        // arrange
        var value = Guid.NewGuid().ToString();

        var result = Result.Fail<string>(value);

        Func<Task<Result<string>>> task = () => Task.FromResult(result);

        // act
        var sut = await (await task()).ToActionResultAsync((v) => $"{v} Arc4u");

        // assert
        sut.Value.Should().BeNull();
        sut.Result.Should().BeOfType<BadRequestObjectResult>();
        ((BadRequestObjectResult)sut.Result).Value.Should().BeOfType<List<ProblemDetails>>();
        var problems = (List<ProblemDetails>)((BadRequestObjectResult)sut.Result).Value;
        problems.Should().NotBeNull();
        problems.Count.Should().Be(1);
        var problem = problems[0];
        problem.Should().NotBeNull();
        problem.Title.Should().Be("Error.");
        problem.Detail.Should().Be(value);
        problem.Status.Should().Be(StatusCodes.Status400BadRequest);
    }
}
