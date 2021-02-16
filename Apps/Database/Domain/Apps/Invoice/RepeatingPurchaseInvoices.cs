// <copyright file="RepeatingPurchaseInvoices.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Database.Domain
{
    public partial class RepeatingPurchaseInvoices
    {
        public static void Daily(ITransaction transaction)
        {
            var repeatingPurchaseInvoices = new RepeatingPurchaseInvoices(transaction).Extent();

            foreach (RepeatingPurchaseInvoice repeatingPurchaseInvoice in repeatingPurchaseInvoices)
            {
                if (repeatingPurchaseInvoice.NextExecutionDate.Date == transaction.Now().Date)
                {
                    repeatingPurchaseInvoice.Repeat();
                }
            }
        }
    }
}
