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

    public class SalesInvoiceItemDeniedPermissionDerivation : DomainDerivation
    {
        public SalesInvoiceItemDeniedPermissionDerivation(M m) : base(m, new Guid("c7ede487-9920-4e47-bb72-1b8f27bdd552")) =>
            this.Patterns = new Pattern[]
        {
            new ChangedPattern(this.M.SalesInvoiceItem.TransitionalDeniedPermissions),
        };

        public override void Derive(IDomainDerivationCycle cycle, IEnumerable<IObject> matches)
        {
            var session = cycle.Session;
            var validation = cycle.Validation;

            foreach (var @this in matches.Cast<SalesInvoiceItem>())
            {
                @this.DeniedPermissions = @this.TransitionalDeniedPermissions;
            }
        }
    }
}
