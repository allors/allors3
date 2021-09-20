// <copyright file="PurchaseOrderBuilderExtensions.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>
// <summary></summary>

namespace Allors.Database.Domain.TestPopulation
{
    using System.Linq;

    public static partial class PurchaseOrderBuilderExtensions
    {
        public static PurchaseOrderBuilder WithDefaults(this PurchaseOrderBuilder @this, Organisation internalOrganisation)
        {
            var faker = @this.Transaction.Faker();

            var postalAddress = new PostalAddressBuilder(@this.Transaction).WithDefaults().Build();
            var supplier = faker.Random.ListItem(internalOrganisation.ActiveSuppliers.ToArray());

            @this.WithDescription(faker.Lorem.Sentence())
                .WithComment(faker.Lorem.Sentence())
                .WithInternalComment(faker.Lorem.Sentence())
                .WithShipToContactPerson(internalOrganisation.CurrentContacts.FirstOrDefault())
                .WithAssignedShipToAddress(internalOrganisation.ShippingAddress)
                .WithBillToContactPerson(internalOrganisation.CurrentContacts.FirstOrDefault())
                .WithAssignedBillToContactMechanism(internalOrganisation.CurrentPartyContactMechanisms.Select(v => v.ContactMechanism).FirstOrDefault())
                .WithTakenViaContactPerson(internalOrganisation.CurrentContacts.FirstOrDefault())
                .WithAssignedTakenViaContactMechanism(internalOrganisation.CurrentPartyContactMechanisms.Select(v => v.ContactMechanism).FirstOrDefault())
                .WithTakenViaSupplier(supplier)
                .WithStoredInFacility(faker.Random.ListItem(internalOrganisation.FacilitiesWhereOwner.ToArray()))
                .WithOrderedBy(internalOrganisation);

            return @this;
        }
    }
}
