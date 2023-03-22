namespace Workspace.ViewModels.WinForms.Services
{
    using Allors.Workspace;
    using Allors.Workspace.Adapters.Remote.SystemText;
    using Workspace.ViewModels.Services;

    public class DatabaseService : IDatabaseService
    {
        public DatabaseService(DatabaseConnection databaseConnection) => this.DatabaseConnection = databaseConnection;

        public DatabaseConnection DatabaseConnection { get; }

        public IWorkspace CreateWorkspace() => this.DatabaseConnection.CreateWorkspace();
    }
}
