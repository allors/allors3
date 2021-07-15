namespace Tests.Workspace
{
    public class MultipleSessionContext : Context
    {
        public MultipleSessionContext(Test test, string name) : base(test, name)
        {
            this.Session1 = test.Workspace.CreateSession();
            this.Session2 = test.Workspace.CreateSession();
        }
    }
}
