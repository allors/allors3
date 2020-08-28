// <copyright file="Domain.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Domain
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Allors.Domain.Derivations;
    using Allors.Meta;

    public static partial class DabaseExtensions
    {
        public class StatementOfWorkCreationDerivation : IDomainDerivation
        {
            public void Derive(ISession session, IChangeSet changeSet, IDomainValidation validation)
            {
                var createdStatementsOfWork = changeSet.Created.Select(session.Instantiate).OfType<StatementOfWork>();

                foreach(var statementOfWork in createdStatementsOfWork)
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

        public static void StatementOfWorkRegisterDerivations(this IDatabase @this)
        {
            @this.DomainDerivationById[new Guid("2f0c24d8-ac90-49e7-8787-21903712272d")] = new StatementOfWorkCreationDerivation();
        }
    }
}
