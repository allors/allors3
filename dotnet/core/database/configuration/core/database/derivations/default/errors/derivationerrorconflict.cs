// <copyright file="DerivationErrorConflict.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Database.Configuration.Derivations.Default
{
    using Database.Derivations;
    using Meta;
    using Resources;

    public class DerivationErrorConflict : DerivationError, IDerivationErrorConflict
    {
        public DerivationErrorConflict(IValidation validation, IDerivationRelation relation)
            : base(validation, new[] { relation }, DomainErrors.DerivationErrorConflict)
        {
        }

        public DerivationErrorConflict(IValidation validation, IObject association, RoleType roleType) :
            this(validation, new DerivationRelation(association, roleType))
        {
        }
    }
}
