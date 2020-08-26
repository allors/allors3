// <copyright file="Domain.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Domain
{
    using System;
    using System.Linq;
    using Allors.Meta;

    public static partial class DabaseExtensions
    {
        public class WebSiteCommunicationsCreationDerivation : IDomainDerivation
        {
            public void Derive(ISession session, IChangeSet changeSet, IDomainValidation validation)
            {
                var createdWebSiteCommunication = changeSet.Created.Select(session.Instantiate).OfType<WebSiteCommunication>();

                foreach(WebSiteCommunication webSiteCommunication in createdWebSiteCommunication)
                {
                    (webSiteCommunication).WorkItemDescription = $"Access website of {webSiteCommunication.ToParty.PartyName} about {webSiteCommunication.Subject}";
                }
            }
        }

        public static void WebSiteCommunicationsRegisterDerivations(this IDatabase @this)
        {
            @this.DomainDerivationById[new Guid("52606ac9-75b7-48f5-9778-e4e94f9207a2")] = new WebSiteCommunicationsCreationDerivation();
        }
    }
}
