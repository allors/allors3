// <copyright file="InvoiceTermBuilderExtensions.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>
// <summary>Defines the MediaTests type.</summary>

namespace Allors.Database.Domain.TestPopulation
{
    using System.Linq;

    public static partial class InvoiceTermBuilderExtensions
    {
        public static InvoiceTermBuilder WithDefaults(this InvoiceTermBuilder @this)
        {
            var faker = @this.Transaction.Faker();

            var allInvoiceItemTypes = @this.Transaction.Extent<InvoiceTermType>().ToList();
            var invoiceItemTypes = allInvoiceItemTypes.Except(allInvoiceItemTypes.Where(v => v.UniqueId == InvoiceTermTypes.PaymentNetDaysId).ToList()).ToList();

            @this.WithTermValue(faker.Lorem.Sentence());
            @this.WithTermType(faker.Random.ListItem(invoiceItemTypes));
            @this.WithDescription(faker.Lorem.Sentence());

            return @this;
        }

        public static InvoiceTermBuilder WithDefaultsForPaymentNetDays(this InvoiceTermBuilder @this)
        {
            var faker = @this.Transaction.Faker();

            @this.WithTermValue(faker.Random.Int(7, 30).ToString());
            @this.WithTermType(new InvoiceTermTypes(@this.Transaction).PaymentNetDays);
            @this.WithDescription(faker.Lorem.Sentence());

            return @this;
        }
    }
}
