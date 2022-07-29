// <copyright file="DerivationErrorGeneric.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Database.Configuration.Derivations.Default
{
    using System;
    using Database.Derivations;
    using Meta;

    public class DerivationErrorGeneric : DerivationError, IDerivationErrorGeneric
    {
        public DerivationErrorGeneric(IValidation validation, IDerivationRelation[] relations, string message, params object[] messageParam)
            : base(validation, relations, message, messageParam)
        {
        }

        public DerivationErrorGeneric(IValidation validation, IDerivationRelation relation, string message, params object[] messageParam)
            : this(validation, relation != null ? new[] { relation } : Array.Empty<IDerivationRelation>(), message, messageParam)
        {
        }

        public DerivationErrorGeneric(IValidation validation, IObject association, RoleType roleType, string message, params object[] messageParam)
            : this(validation, new DerivationRelation(association, roleType), message, messageParam)
        {
        }
    }
}
