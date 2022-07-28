// --------------------------------------------------------------------------------------------------------------------
// <copyright file="EmailAddressBuilderExtensions.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace Allors.Database.Domain.TestPopulation
{
    public static partial class RepeatingSalesInvoiceBuilderExtensions
    {
        public static RepeatingSalesInvoiceBuilder WithDefaults(this RepeatingSalesInvoiceBuilder @this, SalesInvoice salesInvoice)
        {
            @this.WithFrequency(new TimeFrequencies(@this.Transaction).Week)
                 .WithDayOfWeek(new DaysOfWeek(@this.Transaction).Friday)
                 .WithNextExecutionDate(DateTimeFactory.CreateDate(2021, 11, 5))
                 .WithSource(salesInvoice);

            return @this;
        }
    }
}
