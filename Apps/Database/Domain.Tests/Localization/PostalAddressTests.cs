// <copyright file="PostalAddressTests.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>
// <summary>Defines the MediaTests type.</summary>

namespace Allors.Database.Domain.Tests
{
    using System.Collections.Generic;
    using Allors.Database.Derivations;
    using Resources;
    using Xunit;

    public class PostalAddressGeographicBoundaryTests : DomainTest, IClassFixture<Fixture>
    {
        public PostalAddressGeographicBoundaryTests(Fixture fixture) : base(fixture) { }

        [Fact]
        public void GivenGeographicBoundary_WhenDeriving_ThenRequiredRelationsMustExist()
        {
            var country = new Countries(this.Transaction).CountryByIsoCode["BE"];

            new PostalAddressBuilder(this.Transaction).Build();

            Assert.True(this.Transaction.Derive(false).HasErrors);

            this.Transaction.Rollback();

            new PostalAddressBuilder(this.Transaction).WithAddress1("address1").Build();

            Assert.True(this.Transaction.Derive(false).HasErrors);

            this.Transaction.Rollback();

            new PostalAddressBuilder(this.Transaction).WithPostalAddressBoundary(country).Build();

            Assert.True(this.Transaction.Derive(false).HasErrors);

            this.Transaction.Rollback();

            Assert.False(this.Transaction.Derive(false).HasErrors);

            this.Transaction.Rollback();

            new PostalAddressBuilder(this.Transaction).WithAddress1("address1").WithPostalAddressBoundary(country).Build();

            Assert.False(this.Transaction.Derive(false).HasErrors);
        }
    }

    public class PostalAddressRuleTests : DomainTest, IClassFixture<Fixture>
    {
        public PostalAddressRuleTests(Fixture fixture) : base(fixture) { }

        [Fact]
        public void ChangedLocalityThrowValidationError()
        {
            var postalAddress = new PostalAddressBuilder(this.Transaction)
                .WithPostalAddressBoundary(new CityBuilder(this.Transaction).Build())
                .Build();
            this.Transaction.Derive(false);

            postalAddress.Locality = "locality";

            var errors = new List<IDerivationError>(this.Transaction.Derive(false).Errors);
            Assert.Contains(errors, e => e.Message.Equals("AssertExistsAtMostOne: PostalAddress.PostalAddressBoundaries\nPostalAddress.Locality"));
        }

        [Fact]
        public void ChangedRegionThrowValidationError()
        {
            var postalAddress = new PostalAddressBuilder(this.Transaction)
                .WithPostalAddressBoundary(new CityBuilder(this.Transaction).Build())
                .Build();
            this.Transaction.Derive(false);

            postalAddress.Region = "Region";

            var errors = new List<IDerivationError>(this.Transaction.Derive(false).Errors);
            Assert.Contains(errors, e => e.Message.Equals("AssertExistsAtMostOne: PostalAddress.PostalAddressBoundaries\nPostalAddress.Region"));
        }

        [Fact]
        public void ChangedPostalCodeThrowValidationError()
        {
            var postalAddress = new PostalAddressBuilder(this.Transaction)
                .WithPostalAddressBoundary(new CityBuilder(this.Transaction).Build())
                .Build();
            this.Transaction.Derive(false);

            postalAddress.PostalCode = "PostalCode";

            var errors = new List<IDerivationError>(this.Transaction.Derive(false).Errors);
            Assert.Contains(errors, e => e.Message.Equals("AssertExistsAtMostOne: PostalAddress.PostalAddressBoundaries\nPostalAddress.PostalCode"));
        }

        [Fact]
        public void ChangedCountryThrowValidationError()
        {
            var postalAddress = new PostalAddressBuilder(this.Transaction)
                .WithPostalAddressBoundary(new CityBuilder(this.Transaction).Build())
                .Build();
            this.Transaction.Derive(false);

            postalAddress.Country = new CountryBuilder(this.Transaction).Build();

            var errors = new List<IDerivationError>(this.Transaction.Derive(false).Errors);
            Assert.Contains(errors, e => e.Message.Equals("AssertExistsAtMostOne: PostalAddress.PostalAddressBoundaries\nPostalAddress.Country"));
        }

        [Fact]
        public void ChangedPostalAddressBoundariesThrowValidationError()
        {
            var postalAddress = new PostalAddressBuilder(this.Transaction)
                .WithLocality("Locality")
                .Build();
            this.Transaction.Derive(false);

            postalAddress.AddPostalAddressBoundary(new CityBuilder(this.Transaction).Build());

            var errors = new List<IDerivationError>(this.Transaction.Derive(false).Errors);
            Assert.Contains(errors, e => e.Message.Equals("AssertExistsAtMostOne: PostalAddress.PostalAddressBoundaries\nPostalAddress.Locality"));
        }

        [Fact]
        public void ChangedCountryThrowValidationErrorAssertExistsCountry()
        {
            var postalAddress = new PostalAddressBuilder(this.Transaction)
                .WithCountry(new CountryBuilder(this.Transaction).Build())
                .Build();
            this.Transaction.Derive(false);

            postalAddress.RemoveCountry();

            var errors = new List<IDerivationError>(this.Transaction.Derive(false).Errors);
            Assert.Contains(errors, e => e.Message.Equals("AssertExists: PostalAddress.Country"));
        }

        [Fact]
        public void ChangedLocalityThrowValidationErrorAssertExistsLocality()
        {
            var postalAddress = new PostalAddressBuilder(this.Transaction)
                .WithLocality("Locality")
                .Build();
            this.Transaction.Derive(false);

            postalAddress.RemoveLocality();

            var errors = new List<IDerivationError>(this.Transaction.Derive(false).Errors);
            Assert.Contains(errors, e => e.Message.Equals("AssertExists: PostalAddress.Locality"));
        }
    }
}
