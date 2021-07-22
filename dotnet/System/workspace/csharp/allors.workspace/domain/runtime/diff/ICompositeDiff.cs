namespace Allors.Workspace
{
    public interface ICompositeDiff
    {
        IObject OriginalRole { get; }

        IObject CurrentRole { get; }
    }
}
