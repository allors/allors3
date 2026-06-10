// <copyright file="ReactiveFlags.cs" company="Allors bv">
// Copyright (c) Allors bv. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Workspace.Signals.Default.Core
{
    using System;

    [Flags]
    internal enum ReactiveFlags
    {
        None = 0,

        /// <summary>Node's direct dependency changed; must recompute on next read.</summary>
        Dirty = 1 << 0,

        /// <summary>Node's indirect dependency changed; may need recompute if intermediates changed.</summary>
        Pending = 1 << 1,

        /// <summary>Effect is enqueued in the scheduler; prevents duplicate scheduling.</summary>
        Scheduled = 1 << 2,

        /// <summary>Node is currently executing its computation; prevents circular re-entry.</summary>
        Running = 1 << 3,

        /// <summary>Node has been disposed; operations are no-ops.</summary>
        Disposed = 1 << 4,

        /// <summary>Node is being checked during UpdateDependencies; prevents re-visiting.</summary>
        Checking = 1 << 5,
    }
}
