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

    public class StatementOfWorkDerivation : IDomainDerivation
    {
        public Guid Id => new Guid("8307B027-0A59-409F-B47C-B2B2C98267C8");

        public IEnumerable<Pattern> Patterns { get; } = new Pattern[]
        {
            new CreatedPattern(M.StatementOfWork.Class),
        };

        public void Derive(IDomainDerivationCycle cycle, IEnumerable<IObject> matches)
        {
            foreach (var statementOfWork in matches.Cast<StatementOfWork>())
            {
                Sync(statementOfWork);

                var deletePermission = new Permissions(statementOfWork.Strategy.Session).Get(statementOfWork.Meta.ObjectType, statementOfWork.Meta.Delete, Operations.Execute);

                if (statementOfWork.IsDeletable)
                {
                    statementOfWork.RemoveDeniedPermission(deletePermission);
                }
                else
                {
                    statementOfWork.AddDeniedPermission(deletePermission);
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
