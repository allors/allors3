// <copyright file="WorkTaskBuilderExtensions.cs" company="Allors bv">
// Copyright (c) Allors bv. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>
// <summary></summary>

namespace Allors.Database.Domain.TestPopulation
{
    using System;

    public static partial class WorkEffortInvoiceItemBuilderExtensions
    {
        public static WorkEffortInvoiceItemBuilder WithDefaults(this WorkEffortInvoiceItemBuilder @this)
        {
            var faker = @this.Transaction.Faker();

            var types = new InvoiceItemTypes(@this.Transaction).Extent().ToArray();

            @this.WithAmount(Math.Round(faker.Random.Decimal(1M, 10M), 2))
                 .WithInvoiceItemType(new InvoiceItemTypes(@this.Transaction).Service);

            return @this;
        }
    }
}
