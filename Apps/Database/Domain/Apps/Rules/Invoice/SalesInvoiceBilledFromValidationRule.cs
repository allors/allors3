// <copyright file="Domain.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Database.Domain
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Meta;
    using Database.Derivations;
    using Resources;

    public class SalesInvoiceBilledFromValidationRule : Rule
    {
        public SalesInvoiceBilledFromValidationRule(MetaPopulation m) : base(m, new Guid("b6270df4-2d9e-4b38-941d-fe81e8d6dbd6")) =>
            this.Patterns = new Pattern[]
        {
            m.SalesInvoice.RolePattern(v => v.BilledFrom),
        };

        public override void Derive(IDomainDerivationCycle cycle, IEnumerable<IObject> matches)
        {
            var transaction = cycle.Transaction;
            var validation = cycle.Validation;

            foreach (var @this in matches.Cast<SalesInvoice>())
            {
                if (@this.ExistCurrentVersion
                    && @this.CurrentVersion.ExistBilledFrom
                    && @this.BilledFrom != @this.CurrentVersion.BilledFrom)
                {
                    validation.AddError($"{@this} {this.M.SalesInvoice.BilledFrom} {ErrorMessages.InternalOrganisationChanged}");
                }
            }
        }
    }
}
