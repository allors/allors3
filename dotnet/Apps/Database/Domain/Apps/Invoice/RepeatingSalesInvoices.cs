// <copyright file="RepeatingSalesInvoices.cs" company="Allors bv">
// Copyright (c) Allors bv. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Database.Domain
{
    public partial class RepeatingSalesInvoices
    {
        protected override void AppsSecure(Security config)
        {
            var revocations = new Revocations(this.Transaction);
            var permissions = new Permissions(this.Transaction);

            revocations.RepeatingSalesInvoiceDeleteRevocation.DeniedPermissions = new[]
            {
                permissions.Get(this.Meta, this.Meta.Delete),
            };
        }

        public static void Daily(ITransaction transaction)
        {
            foreach (RepeatingSalesInvoice repeatingSalesInvoice in new RepeatingSalesInvoices(transaction).Extent())
            {
                if (repeatingSalesInvoice.NextExecutionDate.Date <= transaction.Now().Date
                    && (!repeatingSalesInvoice.ExistFinalExecutionDate
                    || repeatingSalesInvoice.FinalExecutionDate > transaction.Now().Date)
                    || (repeatingSalesInvoice.FinalExecutionDate == transaction.Now().Date && !repeatingSalesInvoice.FinalExecutionExecuted))
                {
                    repeatingSalesInvoice.Repeat();
                }
            }
        }
    }
}
