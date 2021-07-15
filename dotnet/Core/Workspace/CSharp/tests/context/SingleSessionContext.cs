namespace Tests.Workspace
{
    public class SingleSessionContext : Context
    {
        public SingleSessionContext(Test test) : base(test)
        {
            this.Session1 = test.Workspace.CreateSession();
            this.Session2 = this.Session1;
        }
    }
}
