// <copyright file="CompositesDelta.cs" company="Allors bv">
// Copyright (c) Allors bv. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Workspace.Adapters
{
    using Ranges;

    /// <summary>
    /// A local change to a to-many role, relative to the retained record:
    /// the strategies added to the role and the IDs removed from it.
    /// </summary>
    public sealed class CompositesDelta
    {
        public CompositesDelta(IRange<Strategy> added, IRange<long> removed)
        {
            this.Added = added;
            this.Removed = removed;
        }

        public IRange<Strategy> Added { get; }

        public IRange<long> Removed { get; }
    }
}
