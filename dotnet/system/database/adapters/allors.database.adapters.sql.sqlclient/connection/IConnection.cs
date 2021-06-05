namespace Allors.Database.Adapters.Sql.SqlClient
{
    public interface IConnection
    {
        Command CreateCommand();
        void Commit();
        void Rollback();
    }
}
