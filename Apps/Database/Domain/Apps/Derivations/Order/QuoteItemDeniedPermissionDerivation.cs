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

    public class QuoteItemDeniedPermissionDerivation : DomainDerivation
    {
        public QuoteItemDeniedPermissionDerivation(M m) : base(m, new Guid("04ca22ef-d3b0-40f7-9f60-4c4bf5dc10d7")) =>
            this.Patterns = new Pattern[]
        {
            new ChangedPattern(this.M.QuoteItem.TransitionalDeniedPermissions),
            new ChangedPattern(this.M.Quote.TransitionalDeniedPermissions) { Steps = new IPropertyType[] { this.M.Quote.QuoteItems}},
        };

        public override void Derive(IDomainDerivationCycle cycle, IEnumerable<IObject> matches)
        {
            var session = cycle.Session;
            var validation = cycle.Validation;

            foreach (var @this in matches.Cast<QuoteItem>())
            {
                @this.DeniedPermissions = @this.TransitionalDeniedPermissions;

                var deletePermission = new Permissions(@this.Strategy.Session).Get(@this.Meta.ObjectType, @this.Meta.Delete);
                if (@this.QuoteWhereQuoteItem.IsDeletable())
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
