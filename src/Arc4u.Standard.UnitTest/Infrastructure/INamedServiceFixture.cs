using Arc4u.Dependency;
using Arc4u.Security.Principal;

namespace Arc4u.Standard.UnitTest.Infrastructure
{
    public interface INamedServiceFixture
    {
        INamedServiceScope CreateScope();

        INamedServiceProvider SharedContainer { get; }

        AppPrincipal GetPrincipal();
    }
}
