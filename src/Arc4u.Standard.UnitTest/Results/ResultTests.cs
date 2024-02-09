using System.Threading.Tasks;
using Arc4u.Results;
using Arc4u.Results.Validation;
using AutoFixture;
using AutoFixture.AutoMoq;
using FluentAssertions;
using FluentResults;
using FluentValidation;
using FluentValidation.Results;
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
        }).ConfigureAwait(false);

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
        }).ConfigureAwait(false);

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
        .ConfigureAwait(false);

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
        .ConfigureAwait(false);

        flag.Should().BeFalse();
        sut.Should().BeSameAs(result.Result);
    }

    [Fact]
    [Trait("Category", "CI")]
    public async Task Test_Task_Log_If_Failed_Should()
    {
        var result = Task.FromResult(Result.Fail(""));
        var globalResult = Result.Ok();

        var sut = await result.LogIfFailedAsync(globalResult)
        .ConfigureAwait(false);

        sut.Should().BeSameAs(result.Result);
        globalResult.IsFailed.Should().BeTrue();
        globalResult.Errors.Count.Should().Be(1);
    }

    [Fact]
    [Trait("Category", "CI")]
    public async Task Test_ValueTask_Log_If_Failed_Should()
    {
        var result = ValueTask.FromResult(Result.Fail<string>(""));
        Result<string> globalResult = Result.Ok();

        var sut = await result.LogIfFailedAsync(globalResult)
        .ConfigureAwait(false);

        sut.Should().BeSameAs(result.Result);
        globalResult.IsFailed.Should().BeTrue();
        globalResult.Errors.Count.Should().Be(1);
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

        var validation = validator.Validate(string.Empty);

        validation.IsValid.Should().BeFalse();

        var sut = Result.Fail(validation.Errors.ToFluentResultErrors());

        sut.IsFailed.Should().BeTrue();
        sut.Errors.Count.Should().Be(1);
        sut.Errors[0].Message.Should().Be("A");
        sut.Errors[0].Metadata["Code"].Should().Be("Code");

    }

    private sealed class Validation : AbstractValidator<string>
    {
        public Validation()
        {
            RuleFor(s => s).NotEmpty().WithMessage("A").WithErrorCode("Code");
        }
    }
}
