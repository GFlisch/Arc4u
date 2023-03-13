using System;

namespace Arc4u.Caching.Sql;
public class SqlCacheOption
{
    public string? ConnectionString { get; set; }
    public string TableName { get; set; } = "SqlCache";
    public string SchemaName { get; set; } = "dbo";
    public string? SerializerName { get; set; }
}
