namespace Tests.Workspace
{
    public class SingleSessionContext : Context
    {
        public SingleSessionContext(Test test, string name) : base(test, name)
        {
            this.Session1 = test.Workspace.CreateSession();
            this.Session2 = this.Session1;
        }
    }
}
