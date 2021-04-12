// <copyright file="ChangedRoles.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>
// <summary>Defines the IDomainDerivation type.</summary>

namespace Allors.Database.Derivations
{
    using System;
    using System.Linq.Expressions;
    using Data;
    using Meta;

    public class AssociationPattern<T> : AssociationPattern where T : IComposite
    {
        public AssociationPattern(T objectType, IAssociationType associationType) : base(objectType, associationType)
        {
        }

        public AssociationPattern(T objectType, Func<T, IAssociationType> associationType) : base(objectType, associationType(objectType))
        {
        }

        public AssociationPattern(T objectType, Func<T, IAssociationType> associationType, Expression<Func<T, IComposite>> step) : base(objectType, associationType(objectType)) => this.Path = new[] { step.ToPath(objectType.MetaPopulation) };

        public AssociationPattern(T objectType, Func<T, IAssociationType> associationType, Expression<Func<T, IPropertyType>> step) : base(objectType, associationType(objectType)) => this.Path = new[] { step.ToPath(objectType.MetaPopulation) };
    }
}
