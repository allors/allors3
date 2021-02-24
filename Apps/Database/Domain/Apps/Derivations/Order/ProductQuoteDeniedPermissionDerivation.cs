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

    public class ProductQuoteDeniedPermissionDerivation : DomainDerivation
    {
        public ProductQuoteDeniedPermissionDerivation(M m) : base(m, new Guid("5629cded-4afb-4ca7-9c78-24c998b8698c")) =>
            this.Patterns = new Pattern[]
        {
            new AssociationPattern(this.M.ProductQuote.TransitionalDeniedPermissions),
            new AssociationPattern(this.M.ProductQuote.ValidQuoteItems),
        };

        public override void Derive(IDomainDerivationCycle cycle, IEnumerable<IObject> matches)
        {
            var transaction = cycle.Transaction;
            var validation = cycle.Validation;

            foreach (var @this in matches.Cast<ProductQuote>())
            {
                @this.DeniedPermissions = @this.TransitionalDeniedPermissions;

                var SetReadyPermission = new Permissions(@this.Strategy.Transaction).Get(@this.Meta.ObjectType, @this.Meta.SetReadyForProcessing);

                if (@this.QuoteState.IsCreated)
                {
                    if (@this.ExistValidQuoteItems)
                    {
                        @this.RemoveDeniedPermission(SetReadyPermission);
                    }
                    else
                    {
                        @this.AddDeniedPermission(SetReadyPermission);
                    }
                }

                var deletePermission = new Permissions(@this.Strategy.Transaction).Get(@this.Meta.ObjectType, @this.Meta.Delete);
                if (@this.IsDeletable())
                {
                    @this.RemoveDeniedPermission(deletePermission);
                }
                else
                {
                    @this.AddDeniedPermission(deletePermission);
                }
            }
        }
    }
}
