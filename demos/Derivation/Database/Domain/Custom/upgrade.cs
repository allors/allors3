namespace Allors.Database.Domain
{
    using System.IO;

    public class Upgrade
    {
        private readonly ITransaction transaction;

        private DirectoryInfo DataPath;

        public Upgrade(ITransaction transaction, DirectoryInfo dataPath)
        {
            this.transaction = transaction;
            this.DataPath = dataPath;
        }

        public void Execute()
        {
        }
    }
}
