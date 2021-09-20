// <copyright file="ChangedRoles.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>
// <summary>Defines the IDomainDerivation type.</summary>

namespace Allors.Workspace.Domain.Derivations
{
    using Meta;
    using Workspace.Derivations;

    public class RolePattern : Pattern, IRolePattern
    {
        public RolePattern(IRoleType roleType) => this.RoleType = roleType;

        public RolePattern(IComposite objectType, IRoleType roleType)
        {
            this.RoleType = roleType;
            this.ObjectType = !this.RoleType.AssociationType.ObjectType.Equals(objectType) ? objectType : null;
        }

        public IRoleType RoleType { get; }

        public override IComposite ObjectType { get; }
    }
}
