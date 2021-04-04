// <copyright file="RelationTypeProps.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>
// <summary>Defines the RelationType type.</summary>

namespace Allors.Database.Meta
{
    public sealed partial class UnitProps : MetaObjectProps
    {
        private readonly IUnitBase @class;

        internal UnitProps(IUnitBase @class) => this.@class = @class;

        public override IMetaPopulation MetaPopulation => this.@class.MetaPopulation;

        public override Origin Origin => this.@class.Origin;
    }
}
