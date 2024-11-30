using Arc4u.Configuration;
using Xunit;

namespace Arc4u.UnitTest.Core;


[Trait("Category", "CI")]
public class KevalueSettingsTests
{
    [Fact]
    public void TestGetHashCode()
    {
        var settings1 = new SimpleKeyValueSettings(new Dictionary<string, string> {
            { "key1", "value1" },
            { "key2", "value2" },
            { "key3", "value3" },
            { "key4", "value4" },
            { "key5", "value5" },
            { "key6", "value6" },
        });

        var settings2 = new SimpleKeyValueSettings(new Dictionary<string, string> {
            { "key1", "value1" },
            { "key2", "value2" },
            { "key3", "value3" },
            { "key4", "value4" },
            { "key5", "value5" },
            { "key6", "value6" },
        });

        Assert.Equal(settings1.GetHashCode(), settings2.GetHashCode());

        settings2 = new SimpleKeyValueSettings(new Dictionary<string, string> {
            { "key1", "value1" },
            { "key2", "value2" },
            { "key3", "value3" },
            { "key4", "value4" },
            { "key5", "value5" },
        });

        Assert.NotEqual(settings1.GetHashCode(), settings2.GetHashCode());
    }

    [Fact]
    public void TestEqual()
    {
        var settings1 = new SimpleKeyValueSettings(new Dictionary<string, string> {
            { "key1", "value1" },
            { "key2", "value2" },
            { "key3", "value3" },
            { "key4", "value4" },
            { "key5", "value5" },
            { "key6", "value6" },
        });

        var settings2 = new SimpleKeyValueSettings(new Dictionary<string, string> {
            { "key1", "value1" },
            { "key2", "value2" },
            { "key3", "value3" },
            { "key4", "value4" },
            { "key5", "value5" },
            { "key6", "value6" },
        });

        Assert.True(settings1.Equals(settings2));

        settings2 = new SimpleKeyValueSettings(new Dictionary<string, string> {
            { "key1", "value1" },
            { "key2", "value2" },
            { "key3", "value3" },
            { "key4", "value4" },
            { "key5", "value5" },
        });

        Assert.False(settings1.Equals(settings2));
    }

}
