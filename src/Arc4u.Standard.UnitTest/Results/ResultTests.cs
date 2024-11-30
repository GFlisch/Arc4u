using Arc4u.Results;
using Arc4u.Results.Validation;
using AutoFixture;
using AutoFixture.AutoMoq;
using FluentAssertions;
using FluentResults;
using FluentValidation;
using FluentValidation.Results;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace Arc4u.UnitTest.Results;
public class ResultTests
{
    public ResultTests()
    {
        _fixture = new Fixture();
        _fixture.Customize(new AutoMoqCustomization());
    }

    readonly Fixture _fixture;

    [Fact]
    [Trait("Category", "CI")]
    public void Test_On_Success_Should()
    {
        var result = Result.Ok();
        var flag = false;

        var sut = result.OnSuccess(() =>
        {
            flag = true;
        });

        flag.Should().BeTrue();
        sut.Should().BeSameAs(result);
    }

    [Fact]
    [Trait("Category", "CI")]
    public async Task Test_On_Success_Async_Should()
    {
        var result = Task.FromResult(Result.Ok());
        var flag = false;

        var sut = await result.OnSuccessAsync(async () =>
        {
            await Task.Delay(1).ConfigureAwait(false);
            flag = true;
        });

        flag.Should().BeTrue();
        sut.Should().BeSameAs(result.Result);
    }

    [Fact]
    [Trait("Category", "CI")]
    public void Test_On_Success_With_Failed_Should()
    {
        var result = Result.Fail("");
        var flag = false;

        var sut = result.OnSuccess(() =>
        {
            flag = true;
        });

        flag.Should().BeFalse();
        sut.Should().BeSameAs(result);
    }

    [Fact]
    [Trait("Category", "CI")]
    public async Task Test_On_Success_With_Failed_Async_Should()
    {
        var result = Task.FromResult(Result.Fail(""));
        var flag = false;

        var sut = await result.OnSuccessAsync(async () =>
        {
            await Task.Delay(1).ConfigureAwait(false);
            flag = true;
        });

        flag.Should().BeFalse();
        sut.Should().BeSameAs(result.Result);
    }

    [Fact]
    [Trait("Category", "CI")]
    public void Test_On_Failed_Should()
    {
        var result = Result.Fail("");
        var flag = false;

        var sut = result.OnFailed((errors) =>
        {
            flag = true;
        });

        flag.Should().BeTrue();
        sut.Should().BeSameAs(result);
    }

    [Fact]
    [Trait("Category", "CI")]
    public async Task Test_On_Failed_Async_Should()
    {
        var result = Task.FromResult(Result.Fail(""));
        var flag = false;

        var sut = await result.OnFailedAsync(async (errors) =>
        {
            await Task.Delay(1000).ConfigureAwait(false);
            flag = true;
        })
;

        flag.Should().BeTrue();
        sut.Should().BeSameAs(result.Result);
    }

    [Fact]
    [Trait("Category", "CI")]
    public void Test_On_Failed_With_Success_Should()
    {
        var result = Result.Ok();
        var flag = false;

        var sut = result.OnFailed((errors) =>
        {
            flag = true;
        });

        flag.Should().BeFalse();
        sut.Should().BeSameAs(result);
    }

    [Fact]
    [Trait("Category", "CI")]
    public async Task Test_On_Failed_With_Success_Async_Should()
    {
        var result = Task.FromResult(Result.Ok());
        var flag = false;

        var sut = await result.OnFailedAsync(async (errors) =>
        {
            await Task.Delay(1).ConfigureAwait(false);
            flag = true;
        })
;

        flag.Should().BeFalse();
        sut.Should().BeSameAs(result.Result);
    }

    [Fact]
    [Trait("Category", "CI")]
    public async Task Test_Task_Log_If_Failed_Should()
    {
        var result = Task.FromResult(Result.Fail(""));
        var globalResult = Result.Ok();

        var sut = await result.LogIfFailed()
                              .OnFailed(globalResult);

        sut.Should().BeSameAs(result.Result);
        globalResult.IsFailed.Should().BeTrue();
        globalResult.Errors.Count.Should().Be(1);
    }

    [Fact]
    [Trait("Category", "CI")]
    public async Task Test_ValueTask_Log_If_Failed_Should()
    {
        var result = ValueTask.FromResult(Result.Fail<Guid>(""));
        Result<string> globalResult = Result.Ok();

        var sut = await result.LogIfFailed()
                              .OnFailed((errors) => globalResult.WithErrors(errors));

        sut.Should().BeSameAs(result.Result);
        globalResult.IsFailed.Should().BeTrue();
        globalResult.Errors.Count.Should().Be(1);
    }

    [Fact]
    [Trait("Category", "CI")]
    public async Task Test_Exception_Failed_Should()
    {
        Result globalResult = Result.Ok();

        Func<Task> error = () => throw new DbUpdateException();

        await Result.Try(() => error())
                    .OnFailed(globalResult);

        globalResult.IsFailed.Should().BeTrue();
        globalResult.Errors.Count.Should().Be(1);
        globalResult.Errors[0].Should().BeOfType<ExceptionalError>();
        globalResult.Errors[0].As<ExceptionalError>().Exception.Should().BeOfType<DbUpdateException>();
    }

    [Fact]
    [Trait("Category", "CI")]
    public void MessageDetail_tests()
    {
        ValidationError error = new ValidationFailure() { ErrorMessage = "A", ErrorCode = "Code" };

        IError ierror = error;

        ierror.Message.Should().Be("A");
    }

    [Fact]
    [Trait("Category", "CI")]
    public void Test_Error_Code_Is_Not_Filled_Should()
    {
        var validator = new Validation();

        var sut = validator.ValidateWithResult(string.Empty);

        sut.IsFailed.Should().BeTrue();
        sut.Errors.Count.Should().Be(1);
        var error = sut.Errors[0].As<ValidationError>();
        error.Message.Should().Be("A");
        error.Code.Should().Be("Code");
        error.Metadata["Severity"].Should().Be(Severity.Warning);

    }

    private sealed class Validation : AbstractValidator<string>
    {
        public Validation()
        {
            RuleFor(s => s).NotEmpty().WithMessage("A").WithErrorCode("Code").WithSeverity(Severity.Warning);
        }
    }
}
