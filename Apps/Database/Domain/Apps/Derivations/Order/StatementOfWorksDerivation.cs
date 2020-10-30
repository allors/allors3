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

    public class StatementOfWorkDerivation : DomainDerivation
    {
        public StatementOfWorkDerivation(M m) : base(m, new Guid("8307B027-0A59-409F-B47C-B2B2C98267C8")) =>
            this.Patterns = new Pattern[]
            {
                new CreatedPattern(this.M.StatementOfWork.Class),
            };

        public override void Derive(IDomainDerivationCycle cycle, IEnumerable<IObject> matches)
        {
            foreach (var @this in matches.Cast<StatementOfWork>())
            {
                Sync(@this);

                var deletePermission = new Permissions(@this.Strategy.Session).Get(@this.Meta.ObjectType, @this.Meta.Delete);

                if (@this.IsDeletable)
                {
                    @this.RemoveDeniedPermission(deletePermission);
                }
                else
                {
                    @this.AddDeniedPermission(deletePermission);
                }
            }

            void Sync(StatementOfWork statementOfWork)
            {
                // session.Prefetch(this.SyncPrefetch, this);
                foreach (QuoteItem quoteItem in statementOfWork.QuoteItems)
                {
                    quoteItem.Sync(statementOfWork);
                }
            }
        }
    }
}
