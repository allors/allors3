// <copyright file="ISessionExtensions.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>
// <summary>Defines the ISessionExtension type.</summary>

namespace Allors
{
    using System;
    using Domain.Derivations;

    public static partial class ISessionExtensions
    {
        public static IValidation Derive(this ISession session, bool throwExceptionOnError = true)
        {
            var derivationService = ((DatabaseScope) session.Database.Scope()).DerivationService;
            var derivation = derivationService.CreateDerivation(session);
            var validation = derivation.Derive();
            if (throwExceptionOnError && validation.HasErrors)
            {
                throw new DerivationException(validation);
            }

            return validation;
        }

        public static DateTime Now(this ISession session)
        {
            var now = DateTime.UtcNow;

            var timeService = ((DatabaseScope) session.Database.Scope()).TimeService;
            var timeShift = timeService.Shift;
            if (timeShift != null)
            {
                now = now.Add((TimeSpan)timeShift);
            }

            return now;
        }
    }
}
