using Arc4u.Authorization;
using Arc4u.Security.Principal;
using AwesomeAssertions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Xunit;

namespace Arc4u.UnitTest.Security;

[Trait("Category", "CI")]
public class ScopedOperationsExtensionTests
{
    private static readonly List<Operation> Operations =
    [
        new Operation { ID = 1, Name = "Read" },
        new Operation { ID = 2, Name = "Write" }
    ];

    private static readonly string[] Scopes = ["Tenant1", "Tenant2"];

    private sealed class CustomRequirement : IAuthorizationRequirement;

    [Fact]
    public void AddScopedOperationsPolicy_Registers_ScopedOperationsHandler_As_Scoped()
    {
        var services = new ServiceCollection();

        services.AddScopedOperationsPolicy(Scopes, Operations);

        var descriptor = services.Single(d => d.ServiceType == typeof(IAuthorizationHandler)
                                              && d.ImplementationType == typeof(ScopedOperationsHandler));
        descriptor.Lifetime.Should().Be(ServiceLifetime.Scoped);
    }

    [Fact]
    public void AddScopedOperationsPolicy_Registers_Policy_Per_Operation()
    {
        var services = new ServiceCollection();

        services.AddScopedOperationsPolicy(Scopes, Operations);

        var options = services.BuildServiceProvider().GetRequiredService<IOptions<AuthorizationOptions>>().Value;

        foreach (var operation in Operations)
        {
            var policy = options.GetPolicy(operation.Name);
            policy.Should().NotBeNull();
            policy!.Requirements.Should().ContainSingle()
                .Which.Should().BeOfType<ScopedOperationsRequirement>()
                .Which.Permissions.Should().BeEquivalentTo([(string.Empty, operation.ID)]);
        }
    }

    [Fact]
    public void AddScopedOperationsPolicy_Registers_Policy_Per_Scope_And_Operation()
    {
        var services = new ServiceCollection();

        services.AddScopedOperationsPolicy(Scopes, Operations);

        var options = services.BuildServiceProvider().GetRequiredService<IOptions<AuthorizationOptions>>().Value;

        foreach (var scope in Scopes)
        {
            foreach (var operation in Operations)
            {
                var policy = options.GetPolicy($"{scope}:{operation.Name}");
                policy.Should().NotBeNull();
                policy!.Requirements.Should().ContainSingle()
                    .Which.Should().BeOfType<ScopedOperationsRequirement>()
                    .Which.Permissions.Should().BeEquivalentTo([(scope, operation.ID)]);
            }
        }
    }

    [Fact]
    public void AddScopedOperationsPolicy_With_ExtraPolicies_Registers_Caller_Defined_Policy()
    {
        var services = new ServiceCollection();

        services.AddScopedOperationsPolicy(Scopes, Operations)
            .AddPolicy("ExtraPolicy", p => p.Requirements.Add(new CustomRequirement()));


        var options = services.BuildServiceProvider().GetRequiredService<IOptions<AuthorizationOptions>>().Value;

        var extra = options.GetPolicy("ExtraPolicy");
        extra.Should().NotBeNull();
        extra!.Requirements.Should().ContainSingle().Which.Should().BeOfType<CustomRequirement>();

        // Standard scope/operation policies still registered alongside the extras.
        options.GetPolicy("Read").Should().NotBeNull();
        options.GetPolicy("Tenant1:Write").Should().NotBeNull();
    }

    [Fact]
    public void AddScopedOperationsPolicy_With_ExtraPolicies_Preserves_DefaultPolicy_Customization()
    {
        var services = new ServiceCollection();
        var customDefaultPolicy = new AuthorizationPolicyBuilder()
            .AddRequirements(new CustomRequirement())
            .Build();

        services.AddScopedOperationsPolicy(Scopes, Operations)
            .ConfigureAuthorization(o =>
            {
                o.DefaultPolicy = customDefaultPolicy;
                o.InvokeHandlersAfterFailure = false;
            });

        var options = services.BuildServiceProvider().GetRequiredService<IOptions<AuthorizationOptions>>().Value;

        options.DefaultPolicy.Should().BeSameAs(customDefaultPolicy);
        options.InvokeHandlersAfterFailure.Should().BeFalse();
    }

    [Fact]
    public void AddScopedOperationsPolicy_With_Empty_Scopes_Registers_Only_Base_Operation_Policies()
    {
        var services = new ServiceCollection();

        services.AddScopedOperationsPolicy([], Operations);

        var options = services.BuildServiceProvider().GetRequiredService<IOptions<AuthorizationOptions>>().Value;

        options.GetPolicy("Read").Should().NotBeNull();
        options.GetPolicy("Write").Should().NotBeNull();
        options.GetPolicy("Tenant1:Read").Should().BeNull();
    }

    [Fact]
    public void AddScopedOperationsPolicy_With_Empty_Operations_Registers_No_Policy_But_Still_Registers_Handler()
    {
        var services = new ServiceCollection();

        services.AddScopedOperationsPolicy(Scopes, []);

        var options = services.BuildServiceProvider().GetRequiredService<IOptions<AuthorizationOptions>>().Value;

        options.GetPolicy("Read").Should().BeNull();
        options.GetPolicy("Tenant1:Read").Should().BeNull();

        services.Should().Contain(d => d.ServiceType == typeof(IAuthorizationHandler)
                                       && d.ImplementationType == typeof(ScopedOperationsHandler));
    }

    [Fact]
    public void AddScopedOperationsPolicy_Without_AuthorizationOptions_Does_Not_Throw()
    {
        var services = new ServiceCollection();

        var act = () => services.AddScopedOperationsPolicy(Scopes, Operations);

        act.Should().NotThrow();

        var options = services.BuildServiceProvider().GetRequiredService<IOptions<AuthorizationOptions>>().Value;
        options.GetPolicy("Read").Should().NotBeNull();
    }

    [Fact]
    public void AddAndOperations_Without_Scope_Registers_Policy_With_Empty_Scope()
    {
        var services = new ServiceCollection();

        services.AddScopedOperationsPolicy(Scopes, Operations)
            .AddAndOperations("ReadAndWrite", 1, 2);

        var options = services.BuildServiceProvider().GetRequiredService<IOptions<AuthorizationOptions>>().Value;

        var policy = options.GetPolicy("ReadAndWrite");
        policy.Should().NotBeNull();
        policy!.Requirements.Should().ContainSingle()
            .Which.Should().BeOfType<ScopedOperationsRequirement>()
            .Which.Permissions.Should().BeEquivalentTo([(string.Empty, 1), (string.Empty, 2)]);
    }

    [Fact]
    public void AddAndOperations_With_Scope_Registers_Policy_With_Scope()
    {
        var services = new ServiceCollection();

        services.AddScopedOperationsPolicy(Scopes, Operations)
            .AddAndOperations("ScopedReadAndWrite", "Tenant1", 1, 2);

        var options = services.BuildServiceProvider().GetRequiredService<IOptions<AuthorizationOptions>>().Value;

        var policy = options.GetPolicy("ScopedReadAndWrite");
        policy.Should().NotBeNull();
        policy!.Requirements.Should().ContainSingle()
            .Which.Should().BeOfType<ScopedOperationsRequirement>()
            .Which.Permissions.Should().BeEquivalentTo([("Tenant1", 1), ("Tenant1", 2)]);
    }

    [Fact]
    public void AddAndOperations_With_Null_Name_Throws()
    {
        var services = new ServiceCollection();

        var builder = services.AddScopedOperationsPolicy(Scopes, Operations);

        var act = () => builder.AddAndOperations(null!, 1);

        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void AddAndOperations_With_Empty_Name_Throws()
    {
        var services = new ServiceCollection();

        var builder = services.AddScopedOperationsPolicy(Scopes, Operations);

        var act = () => builder.AddAndOperations("", 1);

        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void AddAndOperations_With_Scope_And_Empty_Name_Throws()
    {
        var services = new ServiceCollection();

        var builder = services.AddScopedOperationsPolicy(Scopes, Operations);

        var act = () => builder.AddAndOperations("", "Tenant1", 1);

        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void AddAndOperations_Chains_Fluently()
    {
        var services = new ServiceCollection();

        services.AddScopedOperationsPolicy(Scopes, Operations)
            .AddAndOperations("Policy1", 1)
            .AddAndOperations("Policy2", "Tenant1", 2);

        var options = services.BuildServiceProvider().GetRequiredService<IOptions<AuthorizationOptions>>().Value;

        options.GetPolicy("Policy1").Should().NotBeNull();
        options.GetPolicy("Policy2").Should().NotBeNull();
    }
}
