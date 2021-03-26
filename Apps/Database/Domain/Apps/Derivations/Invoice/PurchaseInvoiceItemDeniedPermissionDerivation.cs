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

    public class PurchaseInvoiceItemDeniedPermissionDerivation : DomainDerivation
    {
        public PurchaseInvoiceItemDeniedPermissionDerivation(M m) : base(m, new Guid("169b5970-4d22-455a-8034-32fcbe04fc04")) =>
            this.Patterns = new Pattern[]
        {
            new RolePattern(m.PurchaseInvoiceItem, m.PurchaseInvoiceItem.TransitionalDeniedPermissions),
        };

        public override void Derive(IDomainDerivationCycle cycle, IEnumerable<IObject> matches)
        {
            var transaction = cycle.Transaction;
            var validation = cycle.Validation;

            foreach (var @this in matches.Cast<PurchaseInvoiceItem>())
            {
                @this.DeniedPermissions = @this.TransitionalDeniedPermissions;
            }
        }
    }
}
