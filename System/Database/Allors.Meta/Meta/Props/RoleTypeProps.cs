// <copyright file="RoleTypeProps.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>
// <summary>Defines the RoleType type.</summary>

namespace Allors.Database.Meta
{
    public sealed partial class RoleTypeProps : MetaObjectProps
    {
        private readonly IRoleTypeBase roleType;

        internal RoleTypeProps(IRoleTypeBase relationType) => this.roleType = relationType;

        public override IMetaPopulation MetaPopulation => this.roleType.MetaPopulation;

        public override Origin Origin => this.roleType.Origin;
    }
}
