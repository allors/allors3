// <copyright file="DerivationErrorGeneric.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Database.Domain.Derivations.Errors
{
    using System;
    using Allors.Database.Meta;

    public class DerivationErrorGeneric : DerivationError
    {
        public DerivationErrorGeneric(IValidation validation, DerivationRelation[] relations, string message, params object[] messageParam)
            : base(validation, relations, message, messageParam)
        {
        }

        public DerivationErrorGeneric(IValidation validation, DerivationRelation relation, string message, params object[] messageParam)
            : this(validation, relation != null ? new[] { relation } : Array.Empty<DerivationRelation>(), message, messageParam)
        {
        }

        public DerivationErrorGeneric(IValidation validation, IObject association, RoleType roleType, string message, params object[] messageParam)
            : this(validation, new DerivationRelation(association, roleType), message, messageParam)
        {
        }
    }
}
