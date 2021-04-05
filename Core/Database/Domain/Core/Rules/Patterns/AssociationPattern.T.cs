// <copyright file="ChangedRoles.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>
// <summary>Defines the IDomainDerivation type.</summary>

namespace Allors.Database.Derivations
{
    using Antlr.Runtime.Misc;
    using Meta;

    public class AssociationPattern<T> : AssociationPattern where T : IComposite
    {
        public AssociationPattern(T objectType, Func<T, IAssociationType> association, Func<T, IPropertyType> step = null) : base(objectType, association(objectType)) => this.Steps = step != null ? new[] { step(objectType) } : null;
    }
}
