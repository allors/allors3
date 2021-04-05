// <copyright file="RoleTypeProps.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>
// <summary>Defines the RoleType type.</summary>

namespace Allors.Database.Meta
{
    public sealed partial class RoleTypeProps : PropertyTypeProps
    {
        private readonly IRoleTypeBase roleType;

        internal RoleTypeProps(IRoleTypeBase relationType) => this.roleType = relationType;

        public new IObjectTypeBase ObjectType => this.roleType.ObjectType;

        public IAssociationTypeBase AssociationType => this.roleType.AssociationType;

        public IRelationTypeBase RelationType => this.roleType.RelationType;

        public string SingularName => this.roleType.SingularName;

        public string PluralName => this.roleType.PluralName;

        public int? Size => this.roleType.Size;

        public int? Precision => this.roleType.Precision;

        public int? Scale => this.roleType.Scale;

        protected override IMetaObjectBase AsMetaObject => this.roleType;

        protected override IOperandTypeBase AsOperandType => this.roleType;

        protected override IPropertyTypeBase AsPropertyType => this.roleType;

    }
}
