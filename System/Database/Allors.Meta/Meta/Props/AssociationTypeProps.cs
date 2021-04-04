// <copyright file="AssociationTypeProps.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>
// <summary>Defines the AssociationType type.</summary>

namespace Allors.Database.Meta
{
    public sealed partial class AssociationTypeProps : MetaObjectProps
    {
        private readonly IAssociationTypeBase associationType;

        internal AssociationTypeProps(IAssociationTypeBase relationType) => this.associationType = relationType;

        public override IMetaPopulation MetaPopulation => this.associationType.MetaPopulation;

        public override Origin Origin => this.associationType.Origin;
    }
}
