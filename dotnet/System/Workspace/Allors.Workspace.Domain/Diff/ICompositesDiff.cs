namespace Allors.Workspace
{
    using System.Collections.Generic;

    public interface ICompositesDiff : IDiff
    {
        IReadOnlyList<long> OriginalRoleIds { get; }

        IReadOnlyList<long> ChangedRoleIds { get; }

        IReadOnlyList<IStrategy> OriginalRoles { get; }

        IReadOnlyList<IStrategy> ChangedRoles { get; }
    }
}
