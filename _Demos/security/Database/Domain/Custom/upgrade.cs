namespace Allors.Database.Domain
{
    using System;
    using System.IO;

    public class Upgrade
    {
        private readonly ITransaction session;

        private DirectoryInfo DataPath;

        public Upgrade(ITransaction session, DirectoryInfo dataPath)
        {
            this.session = session;
            this.DataPath = dataPath;
        }

        public void Execute()
        {
        }
    }
}
