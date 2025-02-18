// <copyright file="InternalOrganisationAccountingSettings.cs" company="Allors bv">
// Copyright (c) Allors bv. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Database.Domain
{
    using System;
    using System.Linq;

    public partial class InternalOrganisationAccountingSettings
    {
        public void AppsOnBuild(ObjectOnBuild method)
        {
            if (!this.ExistSubAccountCounter)
            {
                this.SubAccountCounter = new CounterBuilder(this.Transaction()).WithUniqueId(Guid.NewGuid()).WithValue(0).Build();
            }

            if (!this.ExistFiscalYearStartMonth)
            {
                this.FiscalYearStartMonth = 1;
            }

            if (!this.ExistFiscalYearStartDay)
            {
                this.FiscalYearStartDay = 1;
            }
        }
        public void AppsOnInit(ObjectOnInit method)
        {
            foreach(InvoiceItemType invoiceItemType in new InvoiceItemTypes(this.Transaction()).Extent())
            {
                if (!this.SettingsForInvoiceItemTypes.Any(v => v.InvoiceItemType.Equals(invoiceItemType)))
                {
                   this.AddSettingsForInvoiceItemType(new InternalOrganisationInvoiceItemTypeSettingsBuilder(this.Transaction())
                        .WithInvoiceItemType(invoiceItemType)
                        .Build());
                }
            }
        }
    }
}
