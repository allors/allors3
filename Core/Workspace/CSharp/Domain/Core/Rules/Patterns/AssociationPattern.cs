// <copyright file="AssociationPattern.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>
// <summary>Defines the IDerivation type.</summary>

namespace Allors.Workspace
{
    using Meta;

    public class AssociationPattern : Pattern
    {
        public AssociationPattern(IAssociationType associationType) => this.AssociationType = associationType;

        public AssociationPattern(IRoleType roleType) : this(roleType.AssociationType) { }

        public IAssociationType AssociationType { get; }
    }
}
