namespace Tests.Workspace
{
    using System;
    using System.Linq;
    using System.Threading.Tasks;
    using Allors;
    using Allors.Workspace;
    using Allors.Workspace.Data;
    using Allors.Workspace.Meta;
    using Xunit;

    public abstract class Context
    {
        protected Context(Test test, string name)
        {
            this.Test = test;
            this.Name = name;
            this.SharedDatabaseWorkspace = this.Test.Profile.CreateWorkspace();
            this.SharedDatabaseSession = this.SharedDatabaseWorkspace.CreateSession();
            this.ExclusiveDatabaseWorkspace = this.Test.Profile.CreateExclusiveWorkspace();
            this.ExclusiveDatabaseSession = this.ExclusiveDatabaseWorkspace.CreateSession();
        }

        public Test Test { get; }

        public string Name { get; }

        public IAsyncDatabaseClient AsyncDatabaseClient => this.Test.AsyncDatabaseClient;

        public ISession Session1 { get; protected set; }

        public ISession Session2 { get; protected set; }

        public IWorkspace SharedDatabaseWorkspace { get; }

        public ISession SharedDatabaseSession { get; }

        public IWorkspace ExclusiveDatabaseWorkspace { get; }

        public ISession ExclusiveDatabaseSession { get; }

        public void Deconstruct(out ISession session1, out ISession session2)
        {
            session1 = this.Session1;
            session2 = this.Session2;
        }

        public async Task<T> Create<T>(ISession session, DatabaseMode mode) where T : class, IObject
        {
            var @class = (IClass)session.Workspace.Configuration.ObjectFactory.GetObjectType<T>();
            if (@class.Origin != Origin.Database)
            {
                throw new ArgumentException($@"Origin is not {Origin.Database}", nameof(mode));
            }

            switch (mode)
            {
                case DatabaseMode.NoPush:
                    return session.Create<T>();
                case DatabaseMode.Push:
                    var pushObject = session.Create<T>();
                    await this.AsyncDatabaseClient.PushAsync(session);
                    return pushObject;
                case DatabaseMode.PushAndPull:
                    var pushAndPullObject = session.Create<T>();
                    var result = await this.AsyncDatabaseClient.PushAsync(session);
                    Assert.False(result.HasErrors);
                    await this.AsyncDatabaseClient.PullAsync(session, new Pull { Object = pushAndPullObject });
                    return pushAndPullObject;
                case DatabaseMode.SharedDatabase:
                    var sharedDatabaseObject = this.SharedDatabaseSession.Create<T>();
                    await this.AsyncDatabaseClient.PushAsync(this.SharedDatabaseSession);
                    var sharedResult = await this.AsyncDatabaseClient.PullAsync(session, new Pull { Object = sharedDatabaseObject });
                    return (T)sharedResult.Objects.Values.First();
                case DatabaseMode.ExclusiveDatabase:
                    var exclusiveDatabaseObject = this.ExclusiveDatabaseSession.Create<T>();
                    await this.AsyncDatabaseClient.PushAsync(this.ExclusiveDatabaseSession);
                    var exclusiveResult = await this.AsyncDatabaseClient.PullAsync(session, new Pull { Object = exclusiveDatabaseObject });
                    return (T)exclusiveResult.Objects.Values.First();
                default:
                    throw new ArgumentOutOfRangeException(nameof(mode), mode, $@"Mode [{string.Join(", ", Enum.GetNames(typeof(DatabaseMode)))}]");
            }
        }

        public async Task<T> Create<T>(ISession session, WorkspaceMode mode) where T : class, IObject
        {
            var @class = (IClass)session.Workspace.Configuration.ObjectFactory.GetObjectType<T>();
            if (@class.Origin != Origin.Workspace)
            {
                throw new ArgumentException($@"Origin is not {Origin.Workspace}", nameof(mode));
            }

            switch (mode)
            {
                case WorkspaceMode.NoPush:
                    return session.Create<T>();
                case WorkspaceMode.Push:
                    var pushObject = session.Create<T>();
                    session.PushToWorkspace();
                    return pushObject;
                case WorkspaceMode.PushAndPull:
                    var pushAndPullObject = session.Create<T>();
                    session.PushToWorkspace();
                    session.PullFromWorkspace();
                    return pushAndPullObject;
                default:
                    throw new ArgumentOutOfRangeException(nameof(mode), mode, $@"Mode [{string.Join(", ", Enum.GetNames(typeof(DatabaseMode)))}]");
            }
        }

        public override string ToString() => this.Name;
    }
}
