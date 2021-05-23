// <copyright file="ISessionExtensions.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>


namespace Allors.Workspace.Domain
{
    using System;
    using Derivations;
    using Workspace;
    using Workspace.Derivations;
    using Workspace.Domain;

    public static partial class ISessionExtensions
    {
        public static IValidation Derive(this ISession session, bool throwExceptionOnError = true)
        {
            var derivationFactory = session.Workspace.Context().DerivationFactory;
            var derivation = derivationFactory.CreateDerivation(session);
            var validation = derivation.Execute();
            if (throwExceptionOnError && validation.HasErrors)
            {
                throw new DerivationException(validation);
            }

            return validation;
        }

        public static DateTime Now(this ISession transaction)
        {
            var now = DateTime.UtcNow;

            var timeService = transaction.Workspace.Context().Time;
            var timeShift = timeService.Shift;
            if (timeShift != null)
            {
                now = now.Add((TimeSpan)timeShift);
            }

            return now;
        }
    }
}
