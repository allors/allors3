// <copyright file="Domain.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Domain
{
    using System;
    using System.Linq;
    using Allors.Meta;
    using Resources;

    public static partial class DabaseExtensions
    {
        public class PaymentExtensionsCreationDerivation : IDomainDerivation
        {
            public void Derive(ISession session, IChangeSet changeSet, IDomainValidation validation)
            {
                var createdPaymentExtensions = changeSet.Created.Select(v=>v.GetObject()).OfType<Payment>();

                foreach(var paymentExtension in createdPaymentExtensions)
                {
                    decimal totalAmountApplied = 0;
                    foreach (PaymentApplication paymentApplication in paymentExtension.PaymentApplications)
                    {
                        totalAmountApplied += paymentApplication.AmountApplied;
                    }

                    if (paymentExtension.ExistAmount && totalAmountApplied > paymentExtension.Amount)
                    {
                        validation.AddError($"{paymentExtension} {M.Payment.Amount} {ErrorMessages.PaymentAmountIsToSmall}");
                    }
                }

            }
        }

        public static void PaymentExtensionsRegisterDerivations(this IDatabase @this)
        {
            @this.DomainDerivationById[new Guid("d31d9e58-2bee-4101-bd23-27ac2c2d4b4e")] = new PaymentExtensionsCreationDerivation();
        }
    }
}
