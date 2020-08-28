// <copyright file="Domain.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Domain
{
    using System;
    using System.Linq;
    using System.Text;
    using Allors.Domain.Derivations;
    using Allors.Meta;
    using Resources;

    public static partial class DabaseExtensions
    {
        public class ServiceExtensionsCreationDerivation : IDomainDerivation
        {
            public void Derive(ISession session, IChangeSet changeSet, IDomainValidation validation)
            {
               var createdServiceExtensions = changeSet.Created.Select(session.Instantiate).OfType<Service>();

                foreach(var serviceExtension in createdServiceExtensions)
                {
                    serviceExtension.BaseOnDeriveVirtualProductPriceComponent();
                }
            }
        }

        public static void ServiceExtensionsRegisterDerivations(this IDatabase @this)
        {
            @this.DomainDerivationById[new Guid("231e54d3-73e6-4985-9e13-49291cdf77e0")] = new ServiceExtensionsCreationDerivation();
        }
    }
}
