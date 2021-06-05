namespace Allors.Database.Adapters.Sql.SqlClient
{
    public interface ISqlParameterCollection
    {
        ISqlParameter this[string name] { get; }
    }
}
