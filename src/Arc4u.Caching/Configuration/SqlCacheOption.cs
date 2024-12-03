namespace Arc4u.Configuration.Sql;
public class SqlCacheOption
{
    public string? ConnectionString { get; set; }
    public string TableName { get; set; } = "SqlCache";
    public string SchemaName { get; set; } = "dbo";
    public string? SerializerName { get; set; }
}
