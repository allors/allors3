// <copyright file="RepeatingSalesInvoice.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Database.Domain
{
    using System;

    public partial class RepeatingSalesInvoice
    {
        public void Repeat()
        {
            var now = this.Strategy.Transaction.Now().Date;
            var monthly = new TimeFrequencies(this.Strategy.Transaction).Month;
            var weekly = new TimeFrequencies(this.Strategy.Transaction).Week;

            if (this.Frequency.Equals(monthly))
            {
                var nextDate = now.AddMonths(1).Date;
                this.Repeat(now, nextDate);
            }

            if (this.Frequency.Equals(weekly))
            {
                var nextDate = now.AddDays(7).Date;
                this.Repeat(now, nextDate);
            }
        }

        private void Repeat(DateTime now, DateTime nextDate)
        {
            if (!this.ExistFinalExecutionDate || nextDate <= this.FinalExecutionDate.Value.Date)
            {
                this.NextExecutionDate = nextDate.Date;

                var nextInvoice = this.Source.AppsCopy(new SalesInvoiceCopy(this.Source));
                this.AddSalesInvoice(nextInvoice);

                this.PreviousExecutionDate = now.Date;
            }
        }
    }
}
