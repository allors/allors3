namespace Allors.Workspace
{
    public interface IUnitDiff
    {
        object OriginalRole { get; }

        object CurrentRole { get; }
    }
}
