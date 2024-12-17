using Arc4u.Security.Principal;

namespace Arc4u.UnitTest.Infrastructure;

public interface IContainerFixture
{
    IServiceProvider CreateScope();

    IServiceProvider SharedContainer { get; }

    AppPrincipal GetPrincipal();
}
