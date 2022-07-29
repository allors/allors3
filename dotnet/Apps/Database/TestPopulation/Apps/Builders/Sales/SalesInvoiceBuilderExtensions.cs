// --------------------------------------------------------------------------------------------------------------------
// <copyright file="EmailAddressBuilderExtensions.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace Allors.Database.Domain.TestPopulation
{
    using System;
    using System.Linq;

    public static partial class SalesInvoiceBuilderExtensions
    {
        public static SalesInvoiceBuilder WithDefaults(this SalesInvoiceBuilder @this, Organisation internalOrganisation)
        {
            var faker = @this.Transaction.Faker();

            var salesInvoiceTypes = new SalesInvoiceTypes(@this.Transaction).Extent().ToArray();
            var customer = faker.PickRandom(internalOrganisation.ActiveCustomers);

            @this.WithSalesInvoiceType(faker.PickRandom(salesInvoiceTypes))
                 .WithBillToCustomer(customer)
                 .WithAssignedBillToContactMechanism(faker.PickRandom(customer.CurrentPartyContactMechanisms.Select(v => v.ContactMechanism)))
                 .WithAdvancePayment(Math.Round(faker.Random.Decimal(1, 10), 2))
                 .WithStore(faker.PickRandom(internalOrganisation.StoresWhereInternalOrganisation));

            return @this;
        }
    }
}
