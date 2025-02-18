// <copyright file="AgreementTermTest.cs" company="Allors bv">
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

    public class AgreementTermTest : DomainTest, IClassFixture<Fixture>
    {
        public AgreementTermTest(Fixture fixture) : base(fixture) { }

        [Fact]
        public void GivenFinancialTerm_WhenDeriving_ThenRequiredRelationsMustExist()
        {
            var builder = new FinancialTermBuilder(this.Transaction);
            var financialTerm = builder.Build();

            Assert.True(this.Derive().HasErrors);

            this.Transaction.Rollback();

            builder.WithDescription("FinancialTerm");
            financialTerm = builder.Build();

            Assert.True(this.Derive().HasErrors);

            builder.WithTermType(new OrderTermTypes(this.Transaction).NonReturnableSalesItem);
            financialTerm = builder.Build();

            Assert.False(this.Derive().HasErrors);

            financialTerm.RemoveDescription();

            Assert.False(this.Derive().HasErrors);
        }

        [Fact]
        public void GivenIncentive_WhenDeriving_ThenRequiredRelationsMustExist()
        {
            var builder = new IncentiveBuilder(this.Transaction);
            var incentive = builder.Build();

            Assert.True(this.Derive().HasErrors);

            this.Transaction.Rollback();

            builder.WithDescription("Incentive");
            incentive = builder.Build();

            Assert.True(this.Derive().HasErrors);

            this.Transaction.Rollback();

            builder.WithTermType(new OrderTermTypes(this.Transaction).NonReturnableSalesItem);
            incentive = builder.Build();

            Assert.False(this.Derive().HasErrors);

            incentive.RemoveDescription();

            Assert.False(this.Derive().HasErrors);
        }

        [Fact]
        public void GivenLegalTerm_WhenDeriving_ThenRequiredRelationsMustExist()
        {
            var builder = new LegalTermBuilder(this.Transaction);
            var legalTerm = builder.Build();

            Assert.True(this.Derive().HasErrors);

            this.Transaction.Rollback();

            builder.WithDescription("LegalTerm");
            legalTerm = builder.Build();

            Assert.True(this.Derive().HasErrors);

            builder.WithTermType(new OrderTermTypes(this.Transaction).NonReturnableSalesItem);
            legalTerm = builder.Build();

            Assert.False(this.Derive().HasErrors);

            legalTerm.RemoveDescription();

            Assert.False(this.Derive().HasErrors);
        }

        [Fact]
        public void GivenThreshold_WhenDeriving_ThenRequiredRelationsMustExist()
        {
            var builder = new ThresholdBuilder(this.Transaction);
            var threshold = builder.Build();

            Assert.True(this.Derive().HasErrors);

            this.Transaction.Rollback();

            builder.WithDescription("Threshold");
            threshold = builder.Build();

            Assert.True(this.Derive().HasErrors);

            builder.WithTermType(new OrderTermTypes(this.Transaction).NonReturnableSalesItem);
            threshold = builder.Build();

            Assert.False(this.Derive().HasErrors);

            threshold.RemoveDescription();

            Assert.False(this.Derive().HasErrors);
        }

        [Fact]
        public void OnCreatedThrowValidationError()
        {
            new InvoiceTermBuilder(this.Transaction).Build();

            var errors = this.Derive().Errors.ToList();
            Assert.Contains(errors, e => e.Message.StartsWith("InvoiceTerm.TermType, InvoiceTerm.Description at least one"));
        }
    }

    public class AgreementTermDerivationTest : DomainTest, IClassFixture<Fixture>
    {
        public AgreementTermDerivationTest(Fixture fixture) : base(fixture) { }

        [Fact]
        public void ChangedDescriptionThrowValidationError()
        {
            var agreementTerm = new InvoiceTermBuilder(this.Transaction)
                .WithDescription("description")
                .Build();

            var errors = this.Derive().Errors.ToList();
            Assert.DoesNotContain(errors, e => e.Message.StartsWith("InvoiceTerm.TermType, InvoiceTerm.Description at least one"));

            agreementTerm.RemoveDescription();

            errors = this.Derive().Errors.ToList();
            Assert.Contains(errors, e => e.Message.StartsWith("InvoiceTerm.TermType, InvoiceTerm.Description at least one"));
        }

        [Fact]
        public void ChangedTermTypeThrowValidationError()
        {
            var agreementTerm = new InvoiceTermBuilder(this.Transaction)
                .WithTermType(new InvoiceTermTypes(this.Transaction).PaymentNetDays)
                .Build();
            this.Derive();

            agreementTerm.RemoveTermType();

            var errors = this.Derive().Errors.OfType<IDerivationErrorAtLeastOne>();
            Assert.Equal(new IRoleType[]
            {
                this.M.InvoiceTerm.TermType,
                this.M.InvoiceTerm.Description,
            }, errors.SelectMany(v => v.RoleTypes).Distinct());
        }
    }
}
