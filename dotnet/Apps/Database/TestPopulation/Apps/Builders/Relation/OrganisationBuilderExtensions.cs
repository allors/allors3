// <copyright file="OrganisationBuilderExtensions.cs" company="Allors bv">
// Copyright (c) Allors bv. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Database.Domain.TestPopulation
{
    using System.Linq;
    using System.Threading;
    using Bogus;
    using Meta;
    using LegalForm = LegalForm;

    public static partial class OrganisationBuilderExtensions
    {
        // Monotonic suffix that makes generated organisation names unique by construction.
        // Bogus' faker.UniqueIndex/IndexGlobal only advance inside the Faker<T>.Generate() pipeline,
        // which these builders do not use, so a dedicated counter is required.
        private static int uniqueNameIndex;

        public static OrganisationBuilder WithDefaults(this OrganisationBuilder @this)
        {
            var m = @this.Transaction.Database.Services.Get<MetaPopulation>();
            var faker = @this.Transaction.Faker();

            var euCountry = new Countries(@this.Transaction).FindBy(m.Country.IsoCode, faker.PickRandom(Countries.EuMemberStates));

            @this.WithName($"{faker.Company.CompanyName()} {Interlocked.Increment(ref uniqueNameIndex)}")
                .WithEuListingState(euCountry)
                .WithLegalForm(faker.Random.ListItem(@this.Transaction.Extent<LegalForm>()))
                .WithLocale(faker.Random.ListItem(@this.Transaction.GetSingleton().Locales.ToArray()))
                .WithTaxNumber($"{euCountry.IsoCode}{faker.Random.Number(99999999)}")
                .WithComment(faker.Lorem.Paragraph());

            return @this;
        }

        public static OrganisationBuilder WithManufacturerDefaults(this OrganisationBuilder @this, Faker faker)
        {
            var company = faker.Company;

            @this.WithName($"{company.CompanyName()} {Interlocked.Increment(ref uniqueNameIndex)}")
                .WithIsManufacturer(true);

            return @this;
        }

        public static OrganisationBuilder WithInternalOrganisationDefaults(this OrganisationBuilder @this)
        {
            var m = @this.Transaction.Database.Services.Get<MetaPopulation>();
            var faker = @this.Transaction.Faker();

            var company = faker.Company;
            var euCountry = new Countries(@this.Transaction).FindBy(m.Country.IsoCode, faker.PickRandom(Countries.EuMemberStates));

            @this.WithName($"{company.CompanyName()} {Interlocked.Increment(ref uniqueNameIndex)}")
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
