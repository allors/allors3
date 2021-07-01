// <copyright file="OrganisationBuilderExtensions.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Database.Domain.TestPopulation
{
    using System.Linq;
    using Bogus;

    public static partial class OrganisationBuilderExtensions
    {
        public static OrganisationBuilder WithDefaults(this OrganisationBuilder @this)
        {
            var m = @this.Transaction.Database.Services().M;
            var faker = @this.Transaction.Faker();

            var euCountry = new Countries(@this.Transaction).FindBy(m.Country.IsoCode, faker.PickRandom(Countries.EuMemberStates));

            @this.WithName(faker.Company.CompanyName())
                .WithEuListingState(euCountry)
                .WithLegalForm(faker.Random.ListItem(@this.Transaction.Extent<LegalForm>()))
                .WithLocale(faker.Random.ListItem(@this.Transaction.GetSingleton().Locales.ToArray()))
                .WithTaxNumber($"{euCountry.IsoCode}{faker.Random.Number(99999999)}")
                .WithComment(faker.Lorem.Paragraph())
                .WithPartyContactMechanism(new PartyContactMechanismBuilder(@this.Transaction)
                .WithUseAsDefault(true)
                .WithContactMechanism(new PostalAddressBuilder(@this.Transaction).WithDefaults().Build())
                .WithContactPurpose(new ContactMechanismPurposes(@this.Transaction).GeneralCorrespondence)
                .WithContactPurpose(new ContactMechanismPurposes(@this.Transaction).ShippingAddress)
                .WithContactPurpose(new ContactMechanismPurposes(@this.Transaction).HeadQuarters)
                .Build())
                .WithPartyContactMechanism(new PartyContactMechanismBuilder(@this.Transaction)
                .WithUseAsDefault(true)
                .WithContactMechanism(new EmailAddressBuilder(@this.Transaction).WithDefaults().Build())
                .WithContactPurpose(new ContactMechanismPurposes(@this.Transaction).GeneralEmail)
                .WithContactPurpose(new ContactMechanismPurposes(@this.Transaction).BillingAddress)
                .Build())
                .WithPartyContactMechanism(new PartyContactMechanismBuilder(@this.Transaction)
                .WithUseAsDefault(true)
                .WithContactMechanism(new WebAddressBuilder(@this.Transaction).WithDefaults().Build())
                .WithContactPurpose(new ContactMechanismPurposes(@this.Transaction).InternetAddress)
                .Build())
                .WithPartyContactMechanism(new PartyContactMechanismBuilder(@this.Transaction)
                .WithUseAsDefault(true)
                .WithContactMechanism(new TelecommunicationsNumberBuilder(@this.Transaction).WithDefaults().Build())
                .WithContactPurpose(new ContactMechanismPurposes(@this.Transaction).GeneralPhoneNumber)
                .WithContactPurpose(new ContactMechanismPurposes(@this.Transaction).BillingInquiriesPhone)
                .Build())
                .WithPartyContactMechanism(new PartyContactMechanismBuilder(@this.Transaction)
                .WithUseAsDefault(true)
                .WithContactMechanism(new TelecommunicationsNumberBuilder(@this.Transaction).WithDefaults().Build())
                .WithContactPurpose(new ContactMechanismPurposes(@this.Transaction).OrderInquiriesPhone)
                .WithContactPurpose(new ContactMechanismPurposes(@this.Transaction).ShippingInquiriesPhone)
                .Build());

            return @this;
        }

        public static OrganisationBuilder WithManufacturerDefaults(this OrganisationBuilder @this, Faker faker)
        {
            var company = faker.Company;

            @this.WithName(company.CompanyName())
                .WithIsManufacturer(true);

            return @this;
        }

        public static OrganisationBuilder WithInternalOrganisationDefaults(this OrganisationBuilder @this)
        {
            var m = @this.Transaction.Database.Services().M;
            var faker = @this.Transaction.Faker();

            var company = faker.Company;
            var euCountry = new Countries(@this.Transaction).FindBy(m.Country.IsoCode, faker.PickRandom(Countries.EuMemberStates));

            @this.WithName(company.CompanyName())
                .WithEuListingState(euCountry)
                .WithLegalForm(faker.Random.ListItem(@this.Transaction.Extent<LegalForm>()))
                .WithLocale(faker.Random.ListItem(@this.Transaction.GetSingleton().Locales.ToArray()))
                .WithTaxNumber($"{euCountry.IsoCode}{faker.Random.Number(99999999)}")
                .WithComment(faker.Lorem.Paragraph())
                .WithIsInternalOrganisation(true);

            return @this;
        }
    }
}
