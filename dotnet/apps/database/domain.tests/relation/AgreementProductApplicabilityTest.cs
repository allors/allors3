// <copyright file="AgreementProductApplicabilityTest.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>
// <summary>Defines the MediaTests type.</summary>

namespace Allors.Database.Domain.Tests
{
    using System.Collections.Generic;
    using Allors.Database.Derivations;
    using Xunit;

    public class AgreementProductApplicabilityTest : DomainTest, IClassFixture<Fixture>
    {
        public AgreementProductApplicabilityTest(Fixture fixture) : base(fixture) { }

        [Fact]
        public void OnCreatedThrowValidationError()
        {
            new AgreementProductApplicabilityBuilder(this.Transaction).Build();

            var errors = new List<IDerivationError>(this.Transaction.Derive(false).Errors);
            Assert.Contains(errors, e => e.Message.StartsWith("AgreementProductApplicability.Agreement, AgreementProductApplicability.AgreementItem at least one"));
        }
    }

    public class AgreementProductApplicabilityDerivationTest : DomainTest, IClassFixture<Fixture>
    {
        public AgreementProductApplicabilityDerivationTest(Fixture fixture) : base(fixture) { }

        [Fact]
        public void ChangedAgreementThrowValidationError()
        {
            var agreementProductApplicability = new AgreementProductApplicabilityBuilder(this.Transaction)
                .WithAgreement(new SalesAgreementBuilder(this.Transaction).Build())
                .Build();

            var errors = new List<IDerivationError>(this.Transaction.Derive(false).Errors);
            Assert.DoesNotContain(errors, e => e.Message.StartsWith("AgreementProductApplicability.Agreement, AgreementProductApplicability.AgreementItem at least one"));

            agreementProductApplicability.RemoveAgreement();

            errors = new List<IDerivationError>(this.Transaction.Derive(false).Errors);
            Assert.Contains(errors, e => e.Message.StartsWith("AgreementProductApplicability.Agreement, AgreementProductApplicability.AgreementItem at least one"));
        }

        [Fact]
        public void ChangedAgreementItemThrowValidationError()
        {
            var agreementProductApplicability = new AgreementProductApplicabilityBuilder(this.Transaction)
                .WithAgreement(new SalesAgreementBuilder(this.Transaction).Build())
                .Build();
            this.Transaction.Derive(false);

            agreementProductApplicability.AgreementItem = new AgreementSectionBuilder(this.Transaction).Build();

            var errors = new List<IDerivationError>(this.Transaction.Derive(false).Errors);
            Assert.Contains(errors, e => e.Message.Equals("AssertExistsAtMostOne: AgreementProductApplicability.Agreement\nAgreementProductApplicability.AgreementItem"));
        }

        [Fact]
        public void ChangedAgreementThrowValidationError_2()
        {
            var agreementProductApplicability = new AgreementProductApplicabilityBuilder(this.Transaction)
                .WithAgreementItem(new AgreementSectionBuilder(this.Transaction).Build())
                .Build();
            this.Transaction.Derive(false);

            agreementProductApplicability.Agreement = new SalesAgreementBuilder(this.Transaction).Build();

            var errors = new List<IDerivationError>(this.Transaction.Derive(false).Errors);
            Assert.Contains(errors, e => e.Message.Equals("AssertExistsAtMostOne: AgreementProductApplicability.Agreement\nAgreementProductApplicability.AgreementItem"));
        }
    }
}
