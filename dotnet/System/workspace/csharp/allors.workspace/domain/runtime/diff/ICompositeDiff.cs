namespace Allors.Workspace
{
    public interface ICompositeDiff : IDiff
    {
        long? OriginalRoleId { get; }

        long? ChangedRoleId { get; }
    }
}
