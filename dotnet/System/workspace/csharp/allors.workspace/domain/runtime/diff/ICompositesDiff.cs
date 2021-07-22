namespace Allors.Workspace
{
    public interface ICompositesDiff
    {
        IObject[] OriginalRole { get; }

        IObject[] CurrentRole { get; }
    }
}
