// <copyright file="DerivationRole.cs" company="Allors bv">
// Copyright (c) Allors bv. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Workspace.Connection
{
    using Meta;

    /// <summary>
    /// A role a derivation error names: the object's id and the relation type.
    /// </summary>
    public sealed class DerivationRole
    {
        public DerivationRole(long objectId, IRelationType relationType)
        {
            this.ObjectId = objectId;
            this.RelationType = relationType;
        }

        public long ObjectId { get; }

        public IRelationType RelationType { get; }
    }
}
