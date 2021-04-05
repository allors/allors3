// <copyright file="RelationTypeProps.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>
// <summary>Defines the RelationType type.</summary>

namespace Allors.Database.Meta
{
    public sealed partial class ClassProps : CompositeProps
    {
        private readonly IClassBase @class;

        internal ClassProps(IClassBase @class) => this.@class = @class;

        #region As
        protected override IMetaObjectBase AsMetaObject => this.@class;

        protected override IObjectTypeBase AsObjectType => this.@class;

        protected override ICompositeBase AsComposite => this.@class;
        #endregion
    }
}
