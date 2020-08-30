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
        public class RequestExtensionsCreationDerivations : IDomainDerivation
        {
            public void Derive(ISession session, IChangeSet changeSet, IDomainValidation validation)
            {
                var createdRequestExtensions = changeSet.Created.Select(v=>v.GetObject()).OfType<Request>();

                foreach (var requestExtensions in createdRequestExtensions)
                {
                    
                    if (!requestExtensions.ExistRecipient)
                    {
                        var internalOrganisations = new Organisations(session).InternalOrganisations();

                        if (internalOrganisations.Count() == 1)
                        {
                            requestExtensions.Recipient = internalOrganisations.Single();
                        }
                    }

                    if (requestExtensions.ExistRecipient && !requestExtensions.ExistRequestNumber)
                    {
                        requestExtensions.RequestNumber = requestExtensions.Recipient.NextRequestNumber(session.Now().Year);
                        (requestExtensions).SortableRequestNumber = requestExtensions.Session().GetSingleton().SortableNumber(requestExtensions.Recipient.RequestNumberPrefix, requestExtensions.RequestNumber, requestExtensions.RequestDate.Year.ToString());
                    }

                    requestExtensions.DeriveInitialObjectState();


                    requestExtensions.AddSecurityToken(new SecurityTokens(session).DefaultSecurityToken);
                }


            }
        }
        public static void RequestExtensionsRegisterDerivations(this IDatabase @this)
        {
            @this.DomainDerivationById[new Guid("fc49f25d-4deb-41d7-8b6b-b34fcfefc42d")] = new RequestExtensionsCreationDerivations();
        }
    }
}
