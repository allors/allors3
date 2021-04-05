// <copyright file="RelationTypeProps.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>
// <summary>Defines the RelationType type.</summary>

namespace Allors.Database.Meta
{
    public sealed partial class UnitProps : ObjectTypeProps
    {
        private readonly IUnitBase unit;

        internal UnitProps(IUnitBase @class) => this.unit = @class;

        protected override IMetaObjectBase AsMetaObject => this.unit;

        protected override IObjectTypeBase AsObjectType => this.unit;
    }
}
