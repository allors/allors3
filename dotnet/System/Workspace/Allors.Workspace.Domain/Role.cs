// <copyright file="Method.cs" company="Allors bv">
// Copyright (c) Allors bv. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Workspace
{
    using Meta;

    public struct Role
    {
        public Role(IObject @object, IRelationType relationType)
        {
            this.Object = @object;
            this.RelationType = relationType;
        }

        public IObject Object { get; }

        public IRelationType RelationType { get; }
    }
}
