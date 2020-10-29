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

    public class ProductQuoteDeniedPermissionDerivation : DomainDerivation
    {
        public ProductQuoteDeniedPermissionDerivation(M m) : base(m, new Guid("5629cded-4afb-4ca7-9c78-24c998b8698c")) =>
            this.Patterns = new Pattern[]
        {
            new ChangedPattern(this.M.ProductQuote.TransitionalDeniedPermissions),
        };

        public override void Derive(IDomainDerivationCycle cycle, IEnumerable<IObject> matches)
        {
            var session = cycle.Session;
            var validation = cycle.Validation;

            foreach (var @this in matches.Cast<ProductQuote>())
            {
                @this.DeniedPermissions = @this.TransitionalDeniedPermissions;
            }
        }
    }
}
