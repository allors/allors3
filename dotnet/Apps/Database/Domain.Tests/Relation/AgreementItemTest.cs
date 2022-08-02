// <copyright file="AgreementItemTest.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>
// <summary>Defines the MediaTests type.</summary>

namespace Allors.Database.Domain.Tests
{
    using Xunit;

    public class AgreementItemTest : DomainTest, IClassFixture<Fixture>
    {
        public AgreementItemTest(Fixture fixture) : base(fixture) { }

        [Fact]
        public void GivenAgreementExhibit_WhenDeriving_ThenDescriptionIsRequired()
        {
            var builder = new AgreementExhibitBuilder(this.Transaction);
            var agreementExhibit = builder.Build();

            Assert.True(this.Derive().HasErrors);

            this.Transaction.Rollback();

            builder.WithDescription("AgreementExhibit");
            agreementExhibit = builder.Build();

            Assert.False(this.Derive().HasErrors);
        }

        [Fact]
        public void GivenAgreementPricingProgram_WhenDeriving_ThenDescriptionIsRequired()
        {
            var builder = new AgreementPricingProgramBuilder(this.Transaction);
            var agreementPricingProgram = builder.Build();

            Assert.True(this.Derive().HasErrors);

            this.Transaction.Rollback();

            builder.WithDescription("AgreementPricingProgram");
            agreementPricingProgram = builder.Build();

            Assert.False(this.Derive().HasErrors);
        }

        [Fact]
        public void GivenAgreementSection_WhenDeriving_ThenDescriptionIsRequired()
        {
            var builder = new AgreementSectionBuilder(this.Transaction);
            var agreementSection = builder.Build();

            Assert.True(this.Derive().HasErrors);

            this.Transaction.Rollback();

            builder.WithDescription("AgreementSection");
            agreementSection = builder.Build();

            Assert.False(this.Derive().HasErrors);
        }

        [Fact]
        public void GivenSubAgreement_WhenDeriving_ThenDescriptionIsRequired()
        {
            var builder = new SubAgreementBuilder(this.Transaction);
            var subAgreement = builder.Build();

            Assert.True(this.Derive().HasErrors);

            this.Transaction.Rollback();

            builder.WithDescription("SubAgreement");
            subAgreement = builder.Build();

            Assert.False(this.Derive().HasErrors);
        }
    }
}
