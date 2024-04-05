using Microsoft.Extensions.Configuration;

namespace Arc4u.Configuration.Store.Internals;

sealed class SectionStoreConfigurationOptions : ISectionStoreConfigurationOptions
{
    private readonly List<(string Key, IValueHolder Value)> _objects;
    private readonly List<IValueBuilder> _sections;

    private interface IValueBuilder
    {
        string Key { get; }
        IValueHolder Build(IConfiguration configuration);
    }

    private sealed class ValueBuilder<TValue> : IValueBuilder
    {
        public string Key { get; }

        public ValueBuilder(string key) => Key = key;

        public IValueHolder Build(IConfiguration configuration)
        {
            var section = configuration.GetRequiredSection(Key);
            var value = section.Get<TValue>();
            return ValueHolder.Create(value);
        }
    }

    public SectionStoreConfigurationOptions()
    {
        _objects = new();
        _sections = new();
    }

    private static void CheckKeyArgument(string key)
    {
        if (string.IsNullOrWhiteSpace(key))
        {
            throw new ArgumentNullException(nameof(key), "The key of a section cannot be null");
        }
    }

    public ISectionStoreConfigurationOptions Add<TValue>(string key, TValue? value)
    {
        CheckKeyArgument(key);
        _objects.Add((key, ValueHolder.Create(value)));
        return this;
    }

    public ISectionStoreConfigurationOptions Add<TValue>(string key)
    {
        CheckKeyArgument(key);
        var builderType = typeof(ValueBuilder<>).MakeGenericType(typeof(TValue));
        // CreateInstance only returns null for nullables, and builderType is not nullable so we can override the null check.
        var valueBuilder = (IValueBuilder)Activator.CreateInstance(builderType, key)!;
        _sections.Add(valueBuilder);
        return this;
    }

    public IReadOnlyList<(string Key, IValueHolder Value)> GetInitialData(IConfigurationBuilder builder)
    {
        if (_sections.Count == 0)
        {
            return _objects;
        }
        else
        {
            var data = new List<(string Key, IValueHolder Value)>(_objects);
            var configurationRoot = builder.Build();
            foreach (var valueBuilder in _sections)
            {
                data.Add((valueBuilder.Key, valueBuilder.Build(configurationRoot)));
            }

            return data;
        }
    }
}
