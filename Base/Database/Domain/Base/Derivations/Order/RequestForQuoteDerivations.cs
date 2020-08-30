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
    using Resources;

    public static partial class DabaseExtensions
    {
        public class RequestForQuoteCreationDerivations : IDomainDerivation
        {
            public void Derive(ISession session, IChangeSet changeSet, IDomainValidation validation)
            {
                var createdRequestForQuote = changeSet.Created.Select(v=>v.GetObject()).OfType<RequestForQuote>();

                foreach (var requestForQuote in createdRequestForQuote)
                {

                    //session.Prefetch(requestForQuote.SyncPrefetch, requestForQuote);
                    foreach (RequestItem requestItem in requestForQuote.RequestItems)
                    {
                        requestItem.Sync(requestForQuote);
                    }

                    if (!requestForQuote.ExistOriginator)
                    {
                        requestForQuote.AddDeniedPermission(new Permissions(requestForQuote.Strategy.Session).Get(requestForQuote.Meta.Class, requestForQuote.Meta.Submit, Operations.Execute));
                    }

                    var deletePermission = new Permissions(requestForQuote.Strategy.Session).Get(requestForQuote.Meta.ObjectType, requestForQuote.Meta.Delete, Operations.Execute);
                    if (requestForQuote.IsDeletable())
                    {
                        requestForQuote.RemoveDeniedPermission(deletePermission);
                    }
                    else
                    {
                        requestForQuote.AddDeniedPermission(deletePermission);
                    }
                }


            }
        }
        public static void RequestForQuoteRegisterDerivations(this IDatabase @this)
        {
            @this.DomainDerivationById[new Guid("5d7db5ae-4bef-4fc2-96f0-63e1906abdda")] = new RequestForQuoteCreationDerivations();
        }
    }
}
