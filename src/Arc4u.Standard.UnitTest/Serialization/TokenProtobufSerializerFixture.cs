using Arc4u.Dependency;
using Arc4u.OAuth2.Token;
using Arc4u.Serializer;
using Arc4u.Standard.UnitTest.Infrastructure;
using Microsoft.Extensions.Configuration;

namespace Arc4u.Standard.UnitTest.Serialization
{
    public class TokenProtobufSerializerFixture : ContainerFixture
    {
        public override string ConfigFile => @"Configs\Basic.json";

        protected override void AddToContainer(IContainerRegistry container, IConfiguration configuration)
        {
            container.Register<IKeyValueSettings, MemorySettings>("Volatile");
            container.Register<IObjectSerialization, ProtoBufSerialization>();
            container.RegisterSingleton<ITokenCache, ApplicationCache>();
            container.RegisterSingleton<CacheHelper, CacheHelper>();
        }
    }

}
