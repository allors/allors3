namespace Allors.Workspace.Domain
{
    public partial class C1
    {
        public override string ToString() => this.ExistName ? this.Name : $"{this.Strategy.Class.SingularName}:{this.Strategy.Id}";
    }
}
