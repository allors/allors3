// <copyright file="DerivationErrorUnique.cs" company="Allors bv">
// Copyright (c) Allors bv. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Database.Configuration.Derivations.Default
{
    using System.Linq;
    using Database.Derivations;
    using Meta;
    using Resources;

    public class DerivationErrorUnique : DerivationError, IDerivationErrorUnique
    {
        public DerivationErrorUnique(IValidation validation, IDerivationRelation[] relations)
            : base(validation, relations, DomainErrors.DerivationErrorUnique)
        {
        }

        public DerivationErrorUnique(IValidation validation, IDerivationRelation relation)
            : this(validation, new[] { relation })
        {
        }

        public DerivationErrorUnique(IValidation validation, IObject association, params IRoleType[] roleTypes) :
            this(validation, roleTypes.Select(v => new DerivationRelation(association, v)).Cast<IDerivationRelation>().ToArray())
        {
        }

        public DerivationErrorUnique(IValidation validation, IObject association, IRoleType roleType) :
            this(validation, new DerivationRelation(association, roleType))
        {
        }
    }
}
