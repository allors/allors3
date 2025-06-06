// <copyright file="ChangedRoles.cs" company="Allors bv">
// Copyright (c) Allors bv. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>
// <summary>Defines the IDomainDerivation type.</summary>

namespace Allors.Database.Domain.Derivations.Rules
{
    using Database.Derivations;
    using Meta;

    public class AssociationPattern : Pattern, IAssociationPattern
    {
        public AssociationPattern(IAssociationType associationType) => this.AssociationType = associationType;

        public AssociationPattern(IRoleType roleType) : this(roleType.AssociationType) { }

        public AssociationPattern(IComposite objectType, IAssociationType associationType) : this(associationType)
        {
            this.AssociationType = associationType;
            this.ObjectType = !this.AssociationType.RoleType.ObjectType.Equals(objectType) ? objectType : null;
        }

        public IAssociationType AssociationType { get; }

        public override IComposite ObjectType { get; }
    }
}
