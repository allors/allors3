namespace Allors.Database.Adapters.Sql.SqlClient
{
    public interface IConnection
    {
        ICommand CreateCommand();

        void Commit();

        void Rollback();
    }
}
