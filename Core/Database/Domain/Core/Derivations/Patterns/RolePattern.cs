// <copyright file="ChangedRoles.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>
// <summary>Defines the IDomainDerivation type.</summary>

namespace Allors.Database.Derivations
{
    using Meta;

    public class RolePattern : Pattern
    {
        public RolePattern(IComposite objectType, IRoleType roleType) : base(objectType) => this.RoleType = roleType;

        public RolePattern(MetaInterface metaInterface, IRoleType roleType) : this(metaInterface.ObjectType, roleType) { }

        public RolePattern(MetaClass metaClass, IRoleType roleType) : this(metaClass.ObjectType, roleType) { }

        public IRoleType RoleType { get; }
    }
}
