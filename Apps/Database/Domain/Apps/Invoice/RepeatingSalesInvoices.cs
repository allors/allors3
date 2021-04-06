// <copyright file="RepeatingSalesInvoices.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Database.Domain
{
    public partial class RepeatingSalesInvoices
    {
        public static void Daily(ITransaction transaction)
        {
            foreach (RepeatingSalesInvoice repeatingSalesInvoice in new RepeatingSalesInvoices(transaction).Extent())
            {
                if (repeatingSalesInvoice.NextExecutionDate.Date == transaction.Now().Date
                    && (!repeatingSalesInvoice.ExistFinalExecutionDate || repeatingSalesInvoice.FinalExecutionDate >= transaction.Now().Date))
                {
                    repeatingSalesInvoice.Repeat();
                }
            }
        }
    }
}
