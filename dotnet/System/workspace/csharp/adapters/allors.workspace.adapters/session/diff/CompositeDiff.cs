// <copyright file="UnitDiff.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Workspace.Adapters
{
    using Meta;

    public class CompositeDiff : Diff, ICompositeDiff
    {
        public CompositeDiff(IRelationType relationType, Strategy association) : base(relationType, association)
        {
        }

        public long? OriginalRoleId { get; set; }

        public long? ChangedRoleId { get; set; }
    }
}
