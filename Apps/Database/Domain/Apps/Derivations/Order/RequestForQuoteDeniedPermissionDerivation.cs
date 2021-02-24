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

    public class RequestForQuoteDeniedPermissionDerivation : DomainDerivation
    {
        public RequestForQuoteDeniedPermissionDerivation(M m) : base(m, new Guid("eb67ef60-1a60-4b52-85ac-979fb9346242")) =>
            this.Patterns = new Pattern[]
        {
            new AssociationPattern(this.M.RequestForQuote.TransitionalDeniedPermissions),
            new AssociationPattern(m.RequestForQuote.RequestItems),
            new AssociationPattern(m.RequestItem.RequestItemState) { Steps =  new IPropertyType[] { m.RequestItem.RequestWhereRequestItem}, OfType = m.RequestForQuote.Class },
            new AssociationPattern(m.Quote.Request) { Steps =  new IPropertyType[] { m.Quote.Request }, OfType = m.RequestForQuote.Class },
        };

        public override void Derive(IDomainDerivationCycle cycle, IEnumerable<IObject> matches)
        {
            var transaction = cycle.Transaction;
            var validation = cycle.Validation;

            foreach (var @this in matches.Cast<RequestForQuote>())
            {
                @this.DeniedPermissions = @this.TransitionalDeniedPermissions;

                if (!@this.ExistOriginator)
                {
                    @this.AddDeniedPermission(new Permissions(@this.Strategy.Transaction).Get(@this.Meta.Class, @this.Meta.Submit));
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
