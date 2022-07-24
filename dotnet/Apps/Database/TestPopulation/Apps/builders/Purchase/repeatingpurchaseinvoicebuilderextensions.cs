// <copyright file="SalesOrderItemBuilderExtensions.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>
// <summary></summary>

using System.Linq;

namespace Allors.Database.Domain.TestPopulation
{
    public static partial class repeatingpurchaseinvoicebuilderextensions
    {
        public static RepeatingPurchaseInvoiceBuilder WithDefaults(this RepeatingPurchaseInvoiceBuilder @this, Organisation organisation)
        {
            var faker = @this.Transaction.Faker();

            @this.WithSupplier(organisation)
                .WithInternalOrganisation(organisation.InternalOrganisationsWhereActiveSupplier.FirstOrDefault())
                .WithFrequency(new TimeFrequencies(@this.Transaction).Week)
                .WithDayOfWeek(new DaysOfWeek(@this.Transaction).Monday)
                .WithNextExecutionDate(DateTimeFactory.CreateDate(2021, 11, 8));

            return @this;
        }
    }
}
