// <copyright file="ITransactionExtensions.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>


namespace Allors.Database.Domain
{
    using System;
    using Derivations;

    public static partial class ITransactionExtensions
    {
        public static IValidation Derive(this ITransaction transaction, bool throwExceptionOnError = true)
        {
            var derivationService = transaction.Database.Services().DerivationFactory;
            var derivation = derivationService.CreateDerivation(transaction);
            var validation = derivation.Derive();
            if (throwExceptionOnError && validation.HasErrors)
            {
                throw new DerivationException(validation);
            }

            return validation;
        }

        public static DateTime Now(this ITransaction transaction)
        {
            var now = DateTime.UtcNow;

            var timeService = transaction.Database.Services().Time;
            var timeShift = timeService.Shift;
            if (timeShift != null)
            {
                now = now.Add((TimeSpan)timeShift);
            }

            return now;
        }
    }
}
