using Arc4u.Dependency;
using Arc4u.Security.Principal;

namespace Arc4u.Standard.UnitTest.Infrastructure
{
    public interface IContainerFixture
    {
        IContainerResolve CreateScope();

        IContainerResolve SharedContainer { get; }

        AppPrincipal GetPrincipal();
    }
}
