namespace Tests.Workspace
{
    using System;
    using System.Linq;
    using System.Threading.Tasks;
    using Allors.Workspace;
    using Allors.Workspace.Data;

    public abstract class Context
    {
        protected Context(Test test, string name)
        {
            this.Test = test;
            this.Name = name;
            this.SharedDatabaseWorkspace = this.Test.Profile.CreateWorkspace();
            this.SharedDatabaseSession = this.SharedDatabaseWorkspace.CreateSession();
            this.ExclusiveDatabase = this.Test.Profile.CreateDatabase();
            this.ExclusiveDatabaseWorkspace = this.ExclusiveDatabase.CreateWorkspace();
            this.ExclusiveDatabaseSession = this.ExclusiveDatabaseWorkspace.CreateSession();
        }


        public Test Test { get; }

        public string Name { get; }

        public ISession Session1 { get; protected set; }

        public ISession Session2 { get; protected set; }

        public IWorkspace SharedDatabaseWorkspace { get; }

        public ISession SharedDatabaseSession { get; }

        public IDatabaseConnection ExclusiveDatabase { get; set; }

        public IWorkspace ExclusiveDatabaseWorkspace { get; }

        public ISession ExclusiveDatabaseSession { get; }

        public void Deconstruct(out ISession session1, out ISession session2)
        {
            session1 = this.Session1;
            session2 = this.Session2;
        }

        public async Task<T> Create<T>(ISession session, Mode mode) where T : class, IObject
        {
            switch (mode)
            {
                case Mode.NoPush:
                    return session.Create<T>();
                case Mode.Push:
                    var pushObject = session.Create<T>();
                    await session.Push();
                    return pushObject;
                case Mode.PushAndPull:
                    var pushAndPullObject = session.Create<T>();
                    await session.Push();
                    await session.Pull(new Pull { Object = pushAndPullObject });
                    return pushAndPullObject;
                case Mode.SharedDatabase:
                    var sharedDatabaseObject = this.SharedDatabaseSession.Create<T>();
                    await this.SharedDatabaseSession.Push();
                    var sharedResult = await session.Pull(new Pull { Object = sharedDatabaseObject });
                    return (T)sharedResult.Objects.Values.First();
                case Mode.ExclusiveDatabase:
                    var exclusiveDatabaseObject = this.ExclusiveDatabaseSession.Create<T>();
                    await this.ExclusiveDatabaseSession.Push();
                    var exclusiveResult = await session.Pull(new Pull { Object = exclusiveDatabaseObject });
                    return (T)exclusiveResult.Objects.Values.First();
                default:
                    throw new ArgumentOutOfRangeException(nameof(mode), mode, $@"Mode [{string.Join(", ", Enum.GetNames(typeof(Mode)))}]");
            }
        }

        public override string ToString() => this.Name;
    }
}
