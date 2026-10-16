namespace Tests.Workspace
{
    using System;
    using System.Linq;
    using System.Threading.Tasks;
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
        }

        public Test Test { get; }

        public string Name { get; }

        public ISession Session1 { get; protected set; }

        public ISession Session2 { get; protected set; }

        public IWorkspace SharedDatabaseWorkspace { get; }

        public ISession SharedDatabaseSession { get; }

        public IWorkspace ExclusiveDatabaseWorkspace { get; private set; }

        public ISession ExclusiveDatabaseSession { get; private set; }

        public void Deconstruct(out ISession session1, out ISession session2)
        {
            session1 = this.Session1;
            session2 = this.Session2;
        }

        public async Task<T> Create<T>(ISession session, DatabaseMode mode) where T : class, IObject
        {
            var @class = (IClass)session.Workspace.Configuration.ObjectFactory.GetObjectType<T>();

            T result;
            switch (mode)
            {
                case DatabaseMode.NoPush:
                    result = session.Create<T>();
                    break;
                case DatabaseMode.Push:
                    var pushObject = session.Create<T>();
                    await session.PushAsync();
                    result = pushObject;
                    break;
                case DatabaseMode.PushAndPull:
                    result = session.Create<T>();
                    var pushResult = await session.PushAsync();
                    Assert.False(pushResult.HasErrors);
                    await session.PullAsync(new Pull { Object = result });
                    break;
                case DatabaseMode.SharedDatabase:
                    var sharedDatabaseObject = this.SharedDatabaseSession.Create<T>();
                    await this.SharedDatabaseSession.PushAsync();
                    var sharedResult = await session.PullAsync(new Pull { Object = sharedDatabaseObject });
                    result = (T)sharedResult.Objects.Values.First();
                    break;
                case DatabaseMode.ExclusiveDatabase:
                    if (this.ExclusiveDatabaseWorkspace == null)
                    {
                        this.ExclusiveDatabaseWorkspace = await this.Test.Profile.CreateExclusiveWorkspaceAsync();
                        this.ExclusiveDatabaseSession = this.ExclusiveDatabaseWorkspace.CreateSession();
                    }
                    var exclusiveDatabaseObject = this.ExclusiveDatabaseSession.Create<T>();
                    await this.ExclusiveDatabaseSession.PushAsync();
                    var exclusiveResult = await session.PullAsync(new Pull { Object = exclusiveDatabaseObject });
                    result = (T)exclusiveResult.Objects.Values.First();
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(mode), mode, $@"Mode [{string.Join(", ", Enum.GetNames(typeof(DatabaseMode)))}]");
            }

            Assert.NotNull(result);
            return result;
        }
        
        public override string ToString() => this.Name;
    }
}
