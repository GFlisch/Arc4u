using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Arc4u.Configuration.Store;

public static class EntityTypeBuilderExtensions
{
    /// <summary>
    /// Configure the <see cref="SectionEntity"/> model.
    /// </summary>
    /// <param name="builder"></param>
    /// <returns></returns>
    public static EntityTypeBuilder<SectionEntity> Configure(this EntityTypeBuilder<SectionEntity> builder)
    {
        // The string holding the section name should have a length limit, but that limit should be large enough to handle realistic section names.
        builder.Property(p => p.Key).HasMaxLength(1024);
        builder.HasKey(p => p.Key);
        builder.Property(p => p.Value);
        return builder;
    }
}

