// <copyright file="IRecord.cs" company="Allors bv">
// Copyright (c) Allors bv. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Workspace.Connection
{
    using Meta;
    using Ranges;

    /// <summary>
    /// A database object as the connection received it: one user's view of the object at the
    /// version it had, immutable. A role is a value: a unit, the id of a composite role, or the
    /// sorted ids of a composites role as an <see cref="IRange{T}"/> of long. A role the user
    /// may not read is absent.
    /// </summary>
    public interface IRecord
    {
        IClass Class { get; }

        long Id { get; }

        long Version { get; }

        /// <summary>
        /// The ids of the grants that apply to the user on this object.
        /// </summary>
        IRange<long> GrantIds { get; }

        /// <summary>
        /// The ids of the revocations that apply to the user on this object.
        /// </summary>
        IRange<long> RevocationIds { get; }

        object GetRole(IRoleType roleType);

        /// <summary>
        /// Whether the user holds the permission on this object: granted by one of the object's
        /// grants and denied by none of its revocations, as the connection holds them now.
        /// </summary>
        bool IsPermitted(long permission);
    }
}
