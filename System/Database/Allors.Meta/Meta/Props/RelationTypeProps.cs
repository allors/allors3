// <copyright file="RelationTypeProps.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>
// <summary>Defines the RelationType type.</summary>

namespace Allors.Database.Meta
{
    public sealed partial class RelationTypeProps : MetaObjectProps
    {
        private readonly IRelationTypeBase relationType;

        internal RelationTypeProps(IRelationTypeBase relationType) => this.relationType = relationType;

        public override IMetaPopulation MetaPopulation => this.relationType.MetaPopulation;

        public override Origin Origin => this.relationType.Origin;
    }
}
