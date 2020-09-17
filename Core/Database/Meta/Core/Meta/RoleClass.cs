// <copyright file="ConcreteRoleType.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>
// <summary>Defines the IObjectType type.</summary>

namespace Allors.Meta
{
    using System;

    public sealed partial class RoleClass : IRoleClass
    {
        public RoleClass( Class @class, RoleType roleType)
        {
            this.Class = @class;
            this.RoleType = roleType;
        }

        public bool IsRequired => this.IsRequiredOverride ?? this.RoleType.IsRequired;

        public bool? IsRequiredOverride { get; set; }

        public bool IsUnique => this.IsUniqueOverride ?? this.RoleType.IsUnique;

        public bool? IsUniqueOverride { get; set; }

        IRoleType IRoleClass.RoleType => this.RoleType;

        public RoleType RoleType { get; }

        public RelationType RelationType => this.RoleType.RelationType;

        IClass IRoleClass.Class => this.Class;

        public Class Class { get; }

        public static implicit operator RoleType(RoleClass roleClass) => roleClass.RoleType;
    }
}
