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

    public class RequestForQuoteDerivation : DomainDerivation
    {
        public RequestForQuoteDerivation(M m) : base(m, new Guid("BD181210-419E-4F87-8B3C-3AEF43711514")) =>
            this.Patterns = new[]
            {
                new CreatedPattern(this.M.RequestForQuote.Class)
            };

        public override void Derive(IDomainDerivationCycle cycle, IEnumerable<IObject> matches)
        {
            foreach (var @this in matches.Cast<RequestForQuote>())
            {
                //session.Prefetch(requestForQuote.SyncPrefetch, requestForQuote);
                foreach (RequestItem requestItem in @this.RequestItems)
                {
                    requestItem.Sync(@this);
                }

                if (!@this.ExistOriginator)
                {
                    @this.AddDeniedPermission(new Permissions(@this.Strategy.Session).Get(@this.Meta.Class, @this.Meta.Submit));
                }

                var deletePermission = new Permissions(@this.Strategy.Session).Get(@this.Meta.ObjectType, @this.Meta.Delete);
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
