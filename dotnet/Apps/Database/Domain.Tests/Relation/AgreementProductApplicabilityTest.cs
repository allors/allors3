// <copyright file="AgreementProductApplicabilityTest.cs" company="Allors bv">
// Copyright (c) Allors bv. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>
// <summary>Defines the MediaTests type.</summary>

namespace Allors.Database.Domain.Tests
{
    using System.Linq;
    using Database.Derivations;
    using Meta;
    using Xunit;

    public class AgreementProductApplicabilityTest : DomainTest, IClassFixture<Fixture>
    {
        public AgreementProductApplicabilityTest(Fixture fixture) : base(fixture) { }

        [Fact]
        public void OnCreatedThrowValidationError()
        {
            new AgreementProductApplicabilityBuilder(this.Transaction).Build();

            var errors = this.Derive().Errors.ToList();
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

            var errors = this.Derive().Errors.ToList();
            Assert.DoesNotContain(errors, e => e.Message.StartsWith("AgreementProductApplicability.Agreement, AgreementProductApplicability.AgreementItem at least one"));

            agreementProductApplicability.RemoveAgreement();

            errors = this.Derive().Errors.ToList();
            Assert.Contains(errors, e => e.Message.StartsWith("AgreementProductApplicability.Agreement, AgreementProductApplicability.AgreementItem at least one"));
        }

        [Fact]
        public void ChangedAgreementItemThrowValidationError()
        {
            var agreementProductApplicability = new AgreementProductApplicabilityBuilder(this.Transaction)
                .WithAgreement(new SalesAgreementBuilder(this.Transaction).Build())
                .Build();
            this.Derive();

            agreementProductApplicability.AgreementItem = new AgreementSectionBuilder(this.Transaction).Build();

            var errors = this.Derive().Errors.OfType<IDerivationErrorAtMostOne>();

            Assert.Equal(new IRoleType[]
               {
                this.M.AgreementProductApplicability.Agreement,
                this.M.AgreementProductApplicability.AgreementItem,
               }, errors.SelectMany(v => v.RoleTypes).Distinct());
        }

        [Fact]
        public void ChangedAgreementThrowValidationError_2()
        {
            var agreementProductApplicability = new AgreementProductApplicabilityBuilder(this.Transaction)
                .WithAgreementItem(new AgreementSectionBuilder(this.Transaction).Build())
                .Build();
            this.Derive();

            agreementProductApplicability.Agreement = new SalesAgreementBuilder(this.Transaction).Build();

            var errors = this.Derive().Errors.OfType<IDerivationErrorAtMostOne>();
            Assert.Equal(new IRoleType[]
            {
                this.M.AgreementProductApplicability.Agreement,
                this.M.AgreementProductApplicability.AgreementItem,
            }, errors.SelectMany(v => v.RoleTypes).Distinct());
        }
    }
}
