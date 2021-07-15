namespace Tests.Workspace
{
    using System;
    using System.Linq;
    using System.Threading.Tasks;
    using Allors.Workspace;
    using Allors.Workspace.Data;

    public abstract class Context
    {
        protected Context(Test test)
        {
            this.Test = test;
            this.OutOfBandWorkspace = this.Test.Profile.CreateWorkspace();
            this.OutOfBandSession = this.OutOfBandWorkspace.CreateSession();
        }
        public Test Test { get; }

        public ISession Session1 { get; protected set; }

        public ISession Session2 { get; protected set; }

        public IWorkspace OutOfBandWorkspace { get; }

        public ISession OutOfBandSession { get; }

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
                case Mode.OutOfBand:
                    var outOfBandObject = this.OutOfBandSession.Create<T>();
                    await this.OutOfBandSession.Push();
                    var result = await session.Pull(new Pull { Object = outOfBandObject });
                    return (T)result.Objects.Values.First();
                default:
                    throw new ArgumentOutOfRangeException(nameof(mode), mode, $@"Mode [{string.Join(", ", Enum.GetNames(typeof(Mode)))}]");
            }
        }
    }
}
