
// <copyright file="WorkTaskCanInvoiceDerivation.cs" company="Allors bv">
// Copyright (c) Allors bv. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Database.Domain
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Database.Derivations;
    using Meta;
    using Derivations.Rules;

    public class WorkTaskCanInvoiceRule : Rule
    {
        public WorkTaskCanInvoiceRule(MetaPopulation m) : base(m, new Guid("17ee3e8a-2430-48db-b712-ba305d488459")) =>
            this.Patterns = new Pattern[]
        {
            m.WorkTask.RolePattern(v => v.DerivationTrigger),
            m.WorkTask.RolePattern(v => v.WorkEffortState),
            m.TimeEntry.AssociationPattern(v => v.TimeSheetWhereTimeEntry, v => v.WorkEffort),
        };

        public override void Derive(ICycle cycle, IEnumerable<IObject> matches)
        {
            var transaction = cycle.Transaction;
            var validation = cycle.Validation;

            foreach (var @this in matches.Cast<WorkTask>())
            {
                @this.DeriveWorkTaskCanInvoice(validation);
            }
        }
    }

    public static class WorkTaskCanInvoiceRuleExtensions
    {
        public static void DeriveWorkTaskCanInvoice(this WorkTask @this, IValidation validation)
        {
            // when proforma invoice is deleted then WorkEffortBillingsWhereWorkEffort do not exist and WorkEffortState is Finished
            if (@this.WorkEffortState.Equals(new WorkEffortStates(@this.Strategy.Transaction).Completed)
                || @this.WorkEffortState.Equals(new WorkEffortStates(@this.Strategy.Transaction).Finished))
            {
                @this.CanInvoice = true;

                if (@this.ExistExecutedBy && @this.ExecutedBy.Equals(@this.Customer))
                {
                    @this.CanInvoice = false;
                }

                foreach (var workEffortBilling in @this.WorkEffortBillingsWhereWorkEffort)
                {
                    if (workEffortBilling.InvoiceItem is SalesInvoiceItem invoiceItem
                        && invoiceItem.ExistSalesInvoiceWhereSalesInvoiceItem
                        && invoiceItem.SalesInvoiceWhereSalesInvoiceItem.SalesInvoiceType.IsSalesInvoice)
                    {
                        // Sales invoice already exists 
                        @this.CanInvoice = false;
                    }
                }

                if (@this.CanInvoice)
                {
                    foreach (TimeEntry timeEntry in @this.ServiceEntriesWhereWorkEffort)
                    {
                        if (!timeEntry.ExistThroughDate)
                        {
                            @this.CanInvoice = false;
                            break;
                        }

                        foreach (var timeEntryBilling in timeEntry.TimeEntryBillingsWhereTimeEntry)
                        {
                            if (timeEntryBilling.InvoiceItem is SalesInvoiceItem invoiceItem
                                && invoiceItem.ExistSalesInvoiceWhereSalesInvoiceItem
                                && invoiceItem.SalesInvoiceWhereSalesInvoiceItem.SalesInvoiceType.IsSalesInvoice)
                            {
                                @this.CanInvoice = false;
                            }
                        }
                    }
                }

                if (@this.ExistWorkEffortWhereChild)
                {
                    @this.CanInvoice = false;
                }

                if (@this.CanInvoice)
                {
                    foreach (var child in @this.Children)
                    {
                        if (!(child.WorkEffortState.Equals(new WorkEffortStates(@this.Strategy.Transaction).Completed)
                            || child.WorkEffortState.Equals(new WorkEffortStates(@this.Strategy.Transaction).Finished)))
                        {
                            @this.CanInvoice = false;
                            break;
                        }
                    }
                }
            }
            else
            {
                @this.CanInvoice = false;
            }
        }
    }
}
