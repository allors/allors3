// <copyright file="Domain.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Domain
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Allors.Meta;

    public class InternalOrganisationExtensionsCreateDerivation : DomainDerivation
    {
        public InternalOrganisationExtensionsCreateDerivation(M m) : base(m, new Guid("f1f61876-17a7-44cc-b58c-b826350554ad")) =>
            this.Patterns = new Pattern[]
            {
                new CreatedPattern(m.InternalOrganisation.Interface),
            };

        public override void Derive(IDomainDerivationCycle cycle, IEnumerable<IObject> matches)
        {
            foreach (var @this in matches.Cast<InternalOrganisation>())
            {
                var singleton = @this.Session().GetSingleton();

                if (@this.IsInternalOrganisation)
                {
                    if (!@this.ExistProductQuoteTemplate)
                    {
                        @this.ProductQuoteTemplate =
                            singleton.CreateOpenDocumentTemplate<Print.ProductQuoteModel.Model>("ProductQuote.odt",
                                singleton.GetResourceBytes("Templates.ProductQuote.odt"));
                    }

                    if (!@this.ExistSalesOrderTemplate)
                    {
                        @this.SalesOrderTemplate =
                            singleton.CreateOpenDocumentTemplate<Print.SalesOrderModel.Model>("SalesOrder.odt",
                                singleton.GetResourceBytes("Templates.SalesOrder.odt"));
                    }

                    if (!@this.ExistPurchaseOrderTemplate)
                    {
                        @this.PurchaseOrderTemplate =
                            singleton.CreateOpenDocumentTemplate<Print.PurchaseOrderModel.Model>("PurchaseOrder.odt",
                                singleton.GetResourceBytes("Templates.PurchaseOrder.odt"));
                    }

                    if (!@this.ExistPurchaseInvoiceTemplate)
                    {
                        @this.PurchaseInvoiceTemplate =
                            singleton.CreateOpenDocumentTemplate<Print.PurchaseInvoiceModel.Model>("PurchaseInvoice.odt",
                                singleton.GetResourceBytes("Templates.PurchaseInvoice.odt"));
                    }

                    if (!@this.ExistSalesInvoiceTemplate)
                    {
                        @this.SalesInvoiceTemplate =
                            singleton.CreateOpenDocumentTemplate<Print.SalesInvoiceModel.Model>("SalesInvoice.odt",
                                singleton.GetResourceBytes("Templates.SalesInvoice.odt"));
                    }

                    if (!@this.ExistWorkTaskTemplate)
                    {
                        @this.WorkTaskTemplate =
                            singleton.CreateOpenDocumentTemplate<Print.WorkTaskModel.Model>("WorkTask.odt",
                                singleton.GetResourceBytes("Templates.WorkTask.odt"));
                    }

                    if (!@this.ExistWorkTaskWorkerTemplate)
                    {
                        @this.WorkTaskWorkerTemplate =
                            singleton.CreateOpenDocumentTemplate<Print.WorkTaskModel.Model>("WorkTaskWorker.odt",
                                singleton.GetResourceBytes("Templates.WorkTaskWorker.odt"));
                    }
                }
            }
        }
    }
}
