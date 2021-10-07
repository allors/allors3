// <copyright file="ObjectsBase.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Database.Domain
{
    using Database.Services;
    using Meta;

    public static class Dependencies
    {
        public static void Create(IDependencyService service, MetaPopulation m)
        {
            void inventoryItemFacilityNameRule(IDependencies dependencies) => dependencies.Add(m.InventoryItem, m.InventoryItem.Facility);
            void partyDisplayPhoneRule(IDependencies dependencies) => dependencies.Add(m.Party, m.Party.PartyContactMechanisms);

            {
                var login = service.GetDependencies("login");
                inventoryItemFacilityNameRule(login);
            }

            {
                var personList = service.GetDependencies("person-list");
                partyDisplayPhoneRule(personList);
            }
        }
    }
}
