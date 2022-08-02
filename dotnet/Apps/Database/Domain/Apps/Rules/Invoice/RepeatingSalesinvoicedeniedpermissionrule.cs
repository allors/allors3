// <copyright file="Domain.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
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

    public class RepeatingSalesInvoiceDeniedPermissionRule : Rule
    {
        public RepeatingSalesInvoiceDeniedPermissionRule(MetaPopulation m) : base(m, new Guid("a486b16e-de22-422f-b3b4-bf154c47b3ce")) =>
            this.Patterns = new Pattern[]
        {
            m.RepeatingSalesInvoice.RolePattern(v => v.SalesInvoices),
        };

        public override void Derive(ICycle cycle, IEnumerable<IObject> matches)
        {
            var transaction = cycle.Transaction;
            var validation = cycle.Validation;

            foreach (var @this in matches.Cast<RepeatingSalesInvoice>())
            {
                @this.DeriveRepeatingSalesInvoiceDeniedPermission(validation);
            }
        }
    }

    public static class RepeatingSalesInvoiceDeniedPermissionRuleExtensions
    {
        public static void DeriveRepeatingSalesInvoiceDeniedPermission(this RepeatingSalesInvoice @this, IValidation validation)
        {
            var revocation = new Revocations(@this.Strategy.Transaction).RepeatingSalesInvoiceDeleteRevocation;
            if (@this.ExistSalesInvoices)
            {
                @this.AddRevocation(revocation);
            }
            else
            {
                @this.RemoveRevocation(revocation);
            }
        }
    }
}
