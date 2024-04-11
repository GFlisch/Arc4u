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
using FluentValidation;
using Arc4u.Results.Validation;

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

public class ProblemDetailsTests
{
    public ProblemDetailsTests()
    {
        _fixture = new Fixture();
        _fixture.Customize(new AutoMoqCustomization());
    }

    readonly Fixture _fixture;

    #region ValueTask
    [Fact]
    [Trait("Category", "CI")]
    // ValueTask<Result<TResult>> result
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
    // ValueTask<Result<TResult>> result
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
        problem.Status.Should().Be(StatusCodes.Status500InternalServerError);
    }

    [Fact]
    [Trait("Category", "CI")]
    // ValueTask<Result<TResult>> result
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
    // ValueTask<Result<TResult>> result
    public async Task Test_ValueTask_Result_To_OnFailed_Without_Mapping_Should()
    {
        // arrange
        var value = Guid.NewGuid().ToString();

        var result = Result.Fail<string>(value);

        Func<ValueTask<Result<string>>> valueTask = () => ValueTask.FromResult(result);

        // act
        var sut = await valueTask().ToActionResultAsync();

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
        problem.Status.Should().Be(StatusCodes.Status500InternalServerError);
    }


    #endregion

    #region Task<Result>
    [Fact]
    [Trait("Category", "CI")]
    // this Task<Result> result
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
        problem.Status.Should().Be(StatusCodes.Status500InternalServerError);
    }

    [Fact]
    [Trait("Category", "CI")]
    // this Task<Result> result
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

    #endregion

    #region Result<TResult>
    [Fact]
    [Trait("Category", "CI")]
    // Result<TResult>
    public async Task Test_Result_T_To_OnSuccess_Without_Mapping_Should()
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
    // Result<TResult>
    public async Task Test_Result_T_To_OnFailed_Without_Mapping_Should()
    {
        // arrange
        var value = Guid.NewGuid().ToString();

        var result = Result.Fail<string>(value);

        Func<Task<Result<string>>> task = () => Task.FromResult(result);

        // act
        var sut = await (await task()).ToActionResultAsync();

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
        problem.Status.Should().Be(StatusCodes.Status500InternalServerError);
    }

    [Fact]
    [Trait("Category", "CI")]
    // Result<TResult>
    public async Task Test_Result_T_To_OnSuccess_With_Mapping_Should()
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
    // Result<TResult>
    public async Task Test_Result_T_To_OnFailed_With_Mapping_Should()
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
        problem.Status.Should().Be(StatusCodes.Status500InternalServerError);
    }


    [Fact]
    [Trait("Category", "CI")]
    // Result<TResult>
    public async Task Test_Result_To_OnFailed_With_Validation_Not_Specific_Async_Should()
    {
        // arrange
        var value = string.Empty;
        var validation = new ValidatorExample();
        //var result = Result.Fail(validation.Validate(value).Errors.ToFluentResultErrors());
        var result = await validation.ValidateWithResultAsync(value);
        validation.ToResult();
        // act
        var sut = await result.ToActionResultAsync();

        // assert
        sut.Value.Should().BeNull();
        sut.Result.Should().BeOfType<BadRequestObjectResult>();
        ((BadRequestObjectResult)sut.Result).Value.Should().BeOfType<List<ProblemDetails>>();
        var problems = (List<ProblemDetails>)((BadRequestObjectResult)sut.Result).Value;
        problems.Should().NotBeNull();
        problems.Count.Should().Be(1);
        var problem = problems[0];
        problem.Should().NotBeNull();
        problem.Title.Should().Be("Error from validation.");
        problem.Detail.Should().Be("'Name' must not be empty.");
        problem.Status.Should().Be(StatusCodes.Status422UnprocessableEntity);
    }

    [Fact]
    [Trait("Category", "CI")]
    // Result<TResult>
    public async Task Test_Result_To_OnFailed_With_Validation_Not_Specific_Should()
    {
        // arrange
        var value = string.Empty;
        var validation = new ValidatorExample();
        //var result = Result.Fail(validation.Validate(value).Errors.ToFluentResultErrors());
        var result = validation.ValidateWithResult(value);
        validation.ToResult();
        // act
        var sut = await result.ToActionResultAsync();

        // assert
        sut.Value.Should().BeNull();
        sut.Result.Should().BeOfType<BadRequestObjectResult>();
        ((BadRequestObjectResult)sut.Result).Value.Should().BeOfType<List<ProblemDetails>>();
        var problems = (List<ProblemDetails>)((BadRequestObjectResult)sut.Result).Value;
        problems.Should().NotBeNull();
        problems.Count.Should().Be(1);
        var problem = problems[0];
        problem.Should().NotBeNull();
        problem.Title.Should().Be("Error from validation.");
        problem.Detail.Should().Be("'Name' must not be empty.");
        problem.Status.Should().Be(StatusCodes.Status422UnprocessableEntity);
    }


    #endregion

    #region Result
    [Fact]
    [Trait("Category", "CI")]
    // Result<TResult>
    public async Task Test_Result_To_OnSuccess_Should()
    {
        // arrange
        var value = Guid.NewGuid().ToString();

        var result = Result.Ok();

        // act
        var sut = await result.ToActionResultAsync();

        // assert
        sut.Should().BeOfType<OkResult>();
    }

    [Fact]
    [Trait("Category", "CI")]
    // Result
    public async Task Test_Result_To_OnFailed_Should()
    {
        // arrange
        var value = Guid.NewGuid().ToString();

        var result = Result.Fail(value);

        // act
        var sut = await result.ToActionResultAsync();

        // assert
        sut.Should().NotBeNull();
        sut.Should().BeOfType<BadRequestObjectResult>();
        ((BadRequestObjectResult)sut).Value.Should().BeOfType<List<ProblemDetails>>();
        var problems = (List<ProblemDetails>)((BadRequestObjectResult)sut).Value;
        problems.Should().NotBeNull();
        problems.Count.Should().Be(1);
        var problem = problems[0];
        problem.Should().NotBeNull();
        problem.Title.Should().Be("Error.");
        problem.Detail.Should().Be(value);
        problem.Status.Should().Be(StatusCodes.Status500InternalServerError);
    }

    [Fact]
    [Trait("Category", "CI")]
    // Result
    public async Task Test_Result_To_OnFailed_With_Validation_Specific_Should()
    {
        // arrange
        var value = Guid.NewGuid().ToString();
        var validation = new ValidatorExample();
        var result = Result.Fail(validation.Validate(value).Errors.ToFluentResultErrors());

        // act
        var sut = await result.ToActionResultAsync();

        // assert
        sut.Should().NotBeNull();
        sut.Should().BeOfType<BadRequestObjectResult>();
        ((BadRequestObjectResult)sut).Value.Should().BeOfType<List<ProblemDetails>>();
        var problems = (List<ProblemDetails>)((BadRequestObjectResult)sut).Value;
        problems.Should().NotBeNull();
        problems.Count.Should().Be(1);
        var problem = problems[0];
        problem.Should().NotBeNull();
        problem.Title.Should().Be("Error from validation.");
        problem.Detail.Should().Be("Problem");
        problem.Status.Should().Be(StatusCodes.Status422UnprocessableEntity);
    }

    #endregion
}
