using Arc4u.Configuration;
using Arc4u.Dependency;
using Arc4u.Dependency.ComponentModel;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Linq;
using Xunit;

namespace Arc4u.Standard.UnitTest
{
    [Trait("Category", "CI")]
    public class NamedServiceTests
    {
        [Fact]
        public void TestCanBeScoped()
        {
            var configuration = ConfigurationHelper.GetConfigurationFromFile(@"Configs\TestParser.json");
            var config = new Config(configuration);


            var container = new ServiceCollection().AddNamedServicesSupport();
            container.AddSingleton(configuration);
            container.AddSingleton(config);
            container.AddScoped<IGenerator, IdGenerator>();

            var service = container.BuildServiceProvider();

            var id1 = service.GetService<IGenerator>().Id;

            using (var c2 = service.CreateScope())
            {
                var id2 = c2.ServiceProvider.GetService<IGenerator>().Id;

                Assert.NotEqual(id1, id2);
                Assert.Equal(id2, c2.ServiceProvider.GetService<IGenerator>().Id);
                Assert.Equal(id1, service.GetService<IGenerator>().Id);
            }
        }

        [Fact]
        public void TestCanBeScopedByName()
        {
            var configuration = ConfigurationHelper.GetConfigurationFromFile(@"Configs\TestParser.json");
            var config = new Config(configuration);


            var container = new ServiceCollection().AddNamedServicesSupport();
            container.AddSingleton(configuration);
            container.AddSingleton(config);
            container.AddScoped<IGenerator, IdGenerator>("Named");

            var service = container.BuildNamedServiceProvider();

            var id1 = service.GetService<IGenerator>("Named").Id;

            using (var c2 = service.CreateScope())
            {
                var id2 = c2.ServiceProvider.GetService<IGenerator>("Named").Id;

                Assert.NotEqual(id1, id2);
                Assert.Equal(id2, c2.ServiceProvider.GetService<IGenerator>("Named").Id);
                Assert.Equal(id1, service.GetService<IGenerator>("Named").Id);
            }
        }

        [Fact]
        void TestRejectedTypeRegister()
        {
            var configuration = ConfigurationHelper.GetConfigurationFromFile(@"Configs\appsettings.RejectedTypes.json");
            var config = new Config(configuration);
            var container = new ServiceCollection().AddNamedServicesSupport().InitializeFromConfig(configuration);
            container.AddSingleton(configuration);
            container.AddSingleton(config);
            var service = container.BuildServiceProvider();

            Assert.NotNull(service.GetService<Config>());
            Assert.Null(service.GetService<IGenerator>());
        }

        [Fact]
        void TestParser()
        {
            var configuration = ConfigurationHelper.GetConfigurationFromFile(@"Configs\TestParser.json");
            var config = new Config(configuration);
            var container = new ServiceCollection().AddNamedServicesSupport().InitializeFromConfig(configuration);
            container.AddSingleton(configuration);
            container.AddSingleton(config);
            var service = container.BuildServiceProvider();

            var byInterface = service.GetService<IGenerator>();
            var byType = service.GetService<TestParser>();
            Assert.NotNull(byInterface);
            Assert.NotNull(byType);
            Assert.NotEqual(byInterface.Id, byType.Id);
        }

        [Fact]
        void TestScopedParser()
        {
            var configuration = ConfigurationHelper.GetConfigurationFromFile(@"Configs\TestScopedParser.json");
            var config = new Config(configuration);
            var container = new ServiceCollection().AddNamedServicesSupport().InitializeFromConfig(configuration);
            container.AddSingleton(configuration);
            container.AddSingleton(config);
            var service = container.BuildServiceProvider();

            var byInterface = service.GetService<IGenerator>();
            var byType = service.GetService<TestScopedParser>();
            Assert.NotNull(byInterface);
            Assert.NotNull(byType);
            Assert.NotEqual(byInterface.Id, byType.Id);

            using (var scope = service.CreateScope())
            {
                var scopedByInterface = scope.ServiceProvider.GetService<IGenerator>();
                var scopedByType = scope.ServiceProvider.GetService<TestScopedParser>();
                Assert.NotNull(scopedByInterface);
                Assert.NotNull(scopedByType);
                Assert.NotEqual(scopedByType.Id, scopedByInterface.Id);

                Assert.NotEqual(scopedByInterface.Id, byInterface.Id);
                Assert.NotEqual(scopedByType.Id, byType.Id);
                Assert.Equal(scopedByInterface.Id, scope.ServiceProvider.GetRequiredService<IGenerator>().Id);
                Assert.Equal(scopedByType.Id, scope.ServiceProvider.GetRequiredService<TestScopedParser>().Id);
            }
        }

        [Fact]
        void TestCompositionRegister()
        {
            var configuration = ConfigurationHelper.GetConfigurationFromFile(@"Configs\EmptyAssemblies.json");
            var config = new Config(configuration);

            var container = new ServiceCollection().AddNamedServicesSupport().InitializeFromConfig(configuration);
            container.AddSingleton(configuration);
            container.AddSingleton(config);
            container.AddTransient<IGenerator, IdGenerator>();
            container.AddSingleton<IGenerator, TestParser>();
            container.AddSingleton<IGenerator, SingletonIdGenerator>();
            container.AddTransient<IGenerator, NamedIdGenerator>("Generator1");
            container.AddSingleton<IGenerator, NamedSingletonIdGenerator>("Generator2");

            var service = container.BuildServiceProvider();

            Assert.NotNull(service.GetService<Config>());
            Assert.True(service.GetServices<IGenerator>().Count() > 1);
        }

        [Fact]
        void TestNullNamingRegister()
        {
            var configuration = ConfigurationHelper.GetConfigurationFromFile(@"Configs\EmptyAssemblies.json");
            var config = new Config(configuration);

            var container = new ServiceCollection().AddNamedServicesSupport().InitializeFromConfig(configuration);
            container.AddSingleton(configuration);
            container.AddSingleton(config);
            Assert.Throws<ArgumentNullException>(() => container.AddTransient<IGenerator, IdGenerator>((string)null));
        }

        [Fact]
        void TestNullNamingGetServicer()
        {
            var configuration = ConfigurationHelper.GetConfigurationFromFile(@"Configs\EmptyAssemblies.json");
            var config = new Config(configuration);

            var container = new ServiceCollection().AddNamedServicesSupport();
            container.AddSingleton(configuration);
            container.AddSingleton(config);
            container.AddTransient<IGenerator, IdGenerator>();
            container.AddTransient<IGenerator, NamedSingletonIdGenerator>("name");
            var service = container.BuildNamedServiceProvider();

            Assert.NotNull(service.GetService<IGenerator>());
            Assert.NotNull(service.GetService<IGenerator>(null));
            Assert.True(service.TryGetService<IGenerator>(out var gen1));
            Assert.True(service.TryGetService<IGenerator>(null, out var gen2));
            Assert.NotNull(service.GetService(typeof(IGenerator)));
            Assert.NotNull(service.GetService(typeof(IGenerator), null));
            Assert.True(service.TryGetService(typeof(IGenerator), out var gen3));
            Assert.True(service.TryGetService(typeof(IGenerator), null, out var gen4));
            var t = service.GetServices<IGenerator>().ToList();
            Assert.True(service.GetServices<IGenerator>().Count() == 1);
            Assert.True(service.GetServices<IGenerator>(null).Count() == 1);
            Assert.True(service.GetServices(typeof(IGenerator)).Count() == 1);
            Assert.True(service.GetServices(typeof(IGenerator), null).Count() == 1);
        }

        [Fact]
        void TestNullNamingMultiGetServiceException()
        {
            var configuration = ConfigurationHelper.GetConfigurationFromFile(@"Configs\EmptyAssemblies.json");
            var config = new Config(configuration);

            var container = new ServiceCollection().AddNamedServicesSupport().InitializeFromConfig(configuration);
            container.AddSingleton(configuration);
            container.AddSingleton(config);
            container.AddTransient<IGenerator, IdGenerator>();
            container.AddSingleton<IGenerator, TestParser>();
            container.AddSingleton<IGenerator, SingletonIdGenerator>();
            container.AddTransient<IGenerator, NamedIdGenerator>("Generator1");
            container.AddSingleton<IGenerator, NamedSingletonIdGenerator>("Generator2");
            var service = container.BuildNamedServiceProvider();

            Assert.True(service.GetServices<IGenerator>().Count() > 1);
            Assert.True(service.GetServices<IGenerator>(null).Count() > 1);
            Assert.True(service.GetServices(typeof(IGenerator)).Count() > 1);
            Assert.True(service.GetServices(typeof(IGenerator), null).Count() > 1);
            //Assert.Throws<MultipleRegistrationException<IGenerator>>(() => service.GetService<IGenerator>());
            //Assert.Throws<MultipleRegistrationException<IGenerator>>(() => service.GetService<IGenerator>(null));
            //Assert.False(service.TryGetService<IGenerator>(out var gen1));
            //Assert.False(service.TryGetService<IGenerator>(null, out var gen2));
            //Assert.Throws<MultipleRegistrationException>(() => service.GetService(typeof(IGenerator)));
            //Assert.Throws<MultipleRegistrationException>(() => service.GetService(typeof(IGenerator), null));
            //Assert.False(service.TryGetService(typeof(IGenerator), out var gen3));
            //Assert.False(service.TryGetService(typeof(IGenerator), null, out var gen4));
        }

        [Fact]
        void TestComponentModelContainerGetServiceOnNonRegisteredType()
        {
            var configuration = ConfigurationHelper.GetConfigurationFromFile(@"Configs\EmptyAssemblies.json");
            var config = new Config(configuration);


            var container = new ServiceCollection().AddNamedServicesSupport().InitializeFromConfig(configuration);
            container.AddSingleton(configuration);
            container.AddSingleton(config);
            var service = container.BuildServiceProvider();

            Assert.NotNull(service.GetService<Config>());
            Assert.Null(service.GetService<IGenerator>());
            Assert.False(service.TryGetService<IGenerator>(out var gen));

        }

        [Fact]
        void TestCreateDelegate()
        {
            ProviderContext.RegisterCreateContainerFunction(() =>
            {
                var configuration = ConfigurationHelper.GetConfigurationFromFile(@"Configs\EmptyAssemblies.json");
                var config = new Config(configuration);

                var container = new ServiceCollection().AddNamedServicesSupport().InitializeFromConfig(configuration);
                container.AddSingleton(configuration);
                container.AddSingleton(config);
                var service = container.BuildNamedServiceProvider();
                return service;
            });

            var configuration = ConfigurationHelper.GetConfigurationFromFile(@"Configs\EmptyAssemblies.json");
            var config = new Config(configuration);

            var service = ProviderContext.CreateContainer();
            Assert.NotNull(service.GetService<Config>());
            Assert.Null(service.GetService<IGenerator>());

            Assert.False(service.TryGetService<IGenerator>(out var gen));
        }



        #region Singleton

        [Fact]
        void TestDIRegisterMultipleShareed()
        {
            var container = new ServiceCollection().AddNamedServicesSupport();
            container.AddSingleton<IGenerator, IdGenerator>();
            container.AddSingleton<IGenerator, NamedIdGenerator>();

            var service = container.BuildServiceProvider();
            Assert.Equal(2, service.GetServices<IGenerator>().Count());
        }

        [Fact]
        void TestDIRegisterMultipleWithNameShared()
        {
            var container = new ServiceCollection().AddNamedServicesSupport();
            container.AddSingleton<IGenerator, NamedIdGenerator>("Gen1");
            container.AddSingleton<IGenerator, IdGenerator>("Gen1");
            var service = container.BuildNamedServiceProvider();

            var generators = service.GetServices<IGenerator>("Gen1");
            Assert.True(generators.Count() == 2);
            Assert.Collection(generators,
                type1 => Assert.IsType<NamedIdGenerator>(type1),
                type2 => Assert.IsType<IdGenerator>(type2));
        }

        [Fact]
        void TestDIRegisterMixMultipleShared()
        {
            var container = new ServiceCollection().AddNamedServicesSupport();
            container.AddSingleton<IGenerator, IdGenerator>();
            container.AddSingleton<IGenerator, NamedIdGenerator>();
            container.AddSingleton<IGenerator, NamedIdGenerator>("Gen1");
            container.AddSingleton<IGenerator, IdGenerator>("Gen1");

            var service = container.BuildNamedServiceProvider();

            Assert.Equal(2, service.GetServices<IGenerator>().Count());

            var generators = service.GetServices<IGenerator>("Gen1");
            Assert.True(generators.Count() == 2);
            Assert.Collection(generators,
                type1 => Assert.IsType<NamedIdGenerator>(type1),
                type2 => Assert.IsType<IdGenerator>(type2));
        }

        [Fact]
        void TestDIAddSingleton()
        {
            var container = new ServiceCollection().AddNamedServicesSupport();
            container.AddSingleton<IGenerator, IdGenerator>();
            container.AddSingleton<IGenerator, IdGenerator>("Gen1");
            var service = container.BuildNamedServiceProvider();

            var idGen1 = service.GetService<IGenerator>();
            var idGen2 = service.GetService<IGenerator>("Gen1");
            Assert.NotEqual(idGen1, idGen2);
            Assert.Equal(idGen1, service.GetService<IGenerator>());
            Assert.Equal(idGen2, service.GetService<IGenerator>("Gen1"));
            // note that it is fortunate that this test doesn't ensure that idGen1 != idGen2, it would otherwise fail. See https://github.com/GFlisch/Arc4u.Guidance.Doc/issues/68
        }
        #endregion Singleton

        #region Factory

        [Fact]
        void TestDIRegisterFactoryOfT()
        {
            var container = new ServiceCollection().AddNamedServicesSupport();
            container.AddTransient<IGenerator>(services => new NamedIdGenerator());
            container.AddTransient<IGenerator>(services => new IdGenerator());

            var service = container.BuildServiceProvider();

            var generators = service.GetServices<IGenerator>().ToList();
            Assert.True(generators.Count() == 2);
            Assert.Collection(generators,
                type1 => Assert.IsType<NamedIdGenerator>(type1),
                type2 => Assert.IsType<IdGenerator>(type2));

            Assert.NotEqual(generators[0].Id, generators[1].Id);
        }

        [Fact]
        void TestDIRegisterFactoryByType()
        {
            var container = new ServiceCollection().AddNamedServicesSupport();
            container.AddTransient(typeof(IGenerator), services => new NamedIdGenerator());
            container.AddTransient(typeof(IGenerator), services => new IdGenerator());

            var service = container.BuildServiceProvider();

            var generators = service.GetServices<IGenerator>().ToList();
            Assert.True(generators.Count() == 2);
            Assert.Collection(generators,
                type1 => Assert.IsType<NamedIdGenerator>(type1),
                type2 => Assert.IsType<IdGenerator>(type2));

            Assert.NotEqual(generators[0].Id, generators[1].Id);
        }

        [Fact]
        void TestDIAddSingletonFactoryOfT()
        {
            var container = new ServiceCollection().AddNamedServicesSupport();
            container.AddSingleton<IGenerator>(services => new NamedIdGenerator());
            container.AddSingleton<IGenerator>(services => new IdGenerator());

            var service = container.BuildServiceProvider();

            var generators = service.GetServices<IGenerator>().ToList();
            Assert.True(generators.Count() == 2);
            Assert.Collection(generators,
                type1 => Assert.IsType<NamedIdGenerator>(type1),
                type2 => Assert.IsType<IdGenerator>(type2));

            Assert.NotEqual(generators[0].Id, generators[1].Id);
        }

        [Fact]
        void TestDIAddSingletonFactoryByType()
        {
            var container = new ServiceCollection().AddNamedServicesSupport();
            container.AddSingleton(typeof(IGenerator), services => new NamedIdGenerator());
            container.AddSingleton(typeof(IGenerator), services => new IdGenerator());

            var service = container.BuildServiceProvider();

            var generators = service.GetServices<IGenerator>().ToList();
            Assert.True(generators.Count() == 2);
            Assert.Collection(generators,
                type1 => Assert.IsType<NamedIdGenerator>(type1),
                type2 => Assert.IsType<IdGenerator>(type2));

            Assert.NotEqual(generators[0].Id, generators[1].Id);
        }
        [Fact]
        void TestDIAddSingletonFactoryOfTIsSingleton()
        {
            var container = new ServiceCollection().AddNamedServicesSupport();
            container.AddSingleton<IGenerator>(services => new NamedIdGenerator());

            var service = container.BuildServiceProvider();

            Assert.Equal(service.GetService<IGenerator>().Id, service.GetService<IGenerator>().Id);
        }

        [Fact]
        void TestDIAddSingletonFactoryByTypeIsSingleton()
        {
            var container = new ServiceCollection().AddNamedServicesSupport();
            container.AddSingleton(typeof(IGenerator), services => new NamedIdGenerator());

            var service = container.BuildServiceProvider();

            Assert.Equal(service.GetService<IGenerator>().Id, service.GetService<IGenerator>().Id);
        }
        #endregion


        [Fact]
        void TestDIRegisterMultipleWithName()
        {
            var container = new ServiceCollection().AddNamedServicesSupport();
            container.AddTransient<IGenerator, NamedIdGenerator>("Gen1");
            container.AddTransient<IGenerator, IdGenerator>("Gen1");
            var service = container.BuildNamedServiceProvider();

            var generators = service.GetServices<IGenerator>("Gen1");
            Assert.True(generators.Count() == 2);
            Assert.Collection(generators,
                type1 => Assert.IsType<NamedIdGenerator>(type1),
                type2 => Assert.IsType<IdGenerator>(type2));
        }

        [Fact]
        void TestDIRegisterMultiple()
        {
            var container = new ServiceCollection().AddNamedServicesSupport();
            container.AddTransient<IGenerator, IdGenerator>();
            container.AddTransient<IGenerator, NamedIdGenerator>();

            var service = container.BuildServiceProvider();

            Assert.Equal(2, service.GetServices<IGenerator>().Count());
        }

        [Fact]
        void TestDIRegisterMixMultiple()
        {
            var container = new ServiceCollection().AddNamedServicesSupport();
            container.AddTransient<IGenerator, IdGenerator>();
            container.AddTransient<IGenerator, NamedIdGenerator>();
            container.AddTransient<IGenerator, NamedIdGenerator>("Gen1");
            container.AddTransient<IGenerator, IdGenerator>("Gen1");

            var service = container.BuildNamedServiceProvider();

            Assert.Equal(2, service.GetServices<IGenerator>().Count());

            var generators = service.GetServices<IGenerator>("Gen1");
            Assert.True(generators.Count() == 2);
            Assert.Collection(generators,
                type1 => Assert.IsType<NamedIdGenerator>(type1),
                type2 => Assert.IsType<IdGenerator>(type2));
        }

        [Fact]
        void TestDIRegisterMefAttribue1()
        {
            var container = new ServiceCollection().AddNamedServicesSupport();
            container.AddExportableTypes(new[] { typeof(IdGenerator) });
            var service = container.BuildServiceProvider();

            Assert.NotNull(service.GetService<IGenerator>());
            var id1 = service.GetService<IGenerator>().Id;
            var id2 = service.GetService<IGenerator>().Id;
            Assert.NotEqual(id1, id2);
        }

        [Fact]
        void TestDIRegisterMefNamedAttribue1()
        {
            var container = new ServiceCollection().AddNamedServicesSupport();
            container.AddExportableTypes(new[] { typeof(NamedIdGenerator) });
            var service = container.BuildNamedServiceProvider();

            service.TryGetService<IGenerator>(out var gen);
            Assert.Null(gen);
            Assert.NotNull(service.GetService<IGenerator>("Generator1"));
            var id1 = service.GetService<IGenerator>("Generator1").Id;
            var id2 = service.GetService<IGenerator>("Generator1").Id;
            Assert.NotEqual(id1, id2);
        }

        [Fact]
        void TestDIRegisterMefSingletonAttribue1()
        {
            var container = new ServiceCollection().AddNamedServicesSupport();
            container.AddExportableTypes(new[] { typeof(SingletonIdGenerator) });
            var service = container.BuildServiceProvider();

            Assert.NotNull(service.GetService<IGenerator>());
            var id1 = service.GetService<IGenerator>().Id;
            var id2 = service.GetService<IGenerator>().Id;
            Assert.Equal(id1, id2);
        }

        [Fact]
        void TestDIRegisterMefNamedSingletonAttribue1()
        {
            var container = new ServiceCollection().AddNamedServicesSupport();
            container.AddExportableTypes(new[] { typeof(NamedSingletonIdGenerator) });
            var service = container.BuildNamedServiceProvider();

            service.TryGetService<IGenerator>(out var gen);
            Assert.Null(gen);
            Assert.NotNull(service.GetService<IGenerator>("Generator2"));
            var id1 = service.GetService<IGenerator>("Generator2").Id;
            var id2 = service.GetService<IGenerator>("Generator2").Id;
            Assert.Equal(id1, id2);
        }

        [Fact]
        void TestDIRegister()
        {
            var container = new ServiceCollection().AddNamedServicesSupport();
            container.AddTransient<IGenerator, IdGenerator>();
            container.AddTransient<IGenerator, IdGenerator>("Gen1");
            var service = container.BuildNamedServiceProvider();

            Assert.NotNull(service.GetService<IGenerator>());
            Assert.NotNull(service.GetService<IGenerator>("Gen1"));
        }


        [Fact]
        void TestDIAddSingletonsOfT()
        {
            var container = new ServiceCollection().AddNamedServicesSupport();
            var instance = new IdGenerator();
            container.AddSingleton<IGenerator>(instance);
            container.AddSingleton<IGenerator>(new NamedIdGenerator());
            var service = container.BuildServiceProvider();

            var generators = service.GetServices<IGenerator>().ToList();
            Assert.Equal(2, generators.Count());
            Assert.NotEqual(generators[0].Id, generators[1].Id);
        }

        [Fact]
        void TestDIAddSingletonsByType()
        {
            var container = new ServiceCollection().AddNamedServicesSupport();
            var instance = new IdGenerator();
            container.AddSingleton(typeof(IGenerator), instance);
            container.AddSingleton(typeof(IGenerator), new NamedIdGenerator());
            var service = container.BuildServiceProvider();

            var generators = service.GetServices<IGenerator>().ToList();
            Assert.Equal(2, generators.Count());
            Assert.NotEqual(generators[0].Id, generators[1].Id);
        }

        [Fact]
        void TestDIAddSingletons2()
        {
            var container = new ServiceCollection().AddNamedServicesSupport();
            var instance = new DualSignletonResolver();
            container.AddSingleton<IBool1>(instance);
            container.AddSingleton<IBool2>(instance);
            var service = container.BuildServiceProvider();

            Assert.NotNull(service.GetService<IBool1>());
            Assert.Equal(service.GetService(typeof(IBool1)), instance);

            Assert.NotNull(service.GetService<IBool2>());
            Assert.NotNull(service.GetService<IBool2>());
            Assert.Equal(service.GetService(typeof(IBool2)), instance);
        }

        [Fact]
        void TestDIAddSingletons3()
        {
            var container = new ServiceCollection().AddNamedServicesSupport();
            var instance = new SingletonIdGenerator();
            container.AddSingleton<IGenerator>(instance);
            container.AddSingleton<IGenerator>(new SingletonIdGenerator(), "Name1");
            container.AddSingleton<IGenerator>(new SingletonIdGenerator(), "Name2");
            var service = container.BuildNamedServiceProvider();

            Assert.NotNull(service.GetService<IGenerator>());
            var n1 = service.GetService(typeof(IGenerator), "Name1");
            var n2 = service.GetService(typeof(IGenerator), "Name2");
            Assert.NotEqual(n1, instance);
            Assert.NotEqual(n2, instance);
            Assert.NotEqual(n1, n2);
        }

        [Fact]
        void TestDIAddSingletons4()
        {
            var container = new ServiceCollection().AddNamedServicesSupport();
            var instance = new SingletonIdGenerator();
            container.AddSingleton<IGenerator>(instance);
            container.AddSingleton<IGenerator>(new SingletonIdGenerator(), "Name");
            container.AddSingleton<IGenerator>(new SingletonIdGenerator(), "Name");
            var service = container.BuildNamedServiceProvider();

            Assert.NotNull(service.GetService<IGenerator>());
            Assert.Throws<IndexOutOfRangeException>(() => service.GetService(typeof(IGenerator), "Name"));
            var instances = service.GetServices(typeof(IGenerator), "Name");
            Assert.True(instances.Count() == 2);
            Assert.NotEqual(instances.First(), instances.Last());
        }
    }
}
