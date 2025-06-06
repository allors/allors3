// <copyright file="OrderTermTests.cs" company="Allors bv">
// Copyright (c) Allors bv. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>
// <summary>Defines the MediaTests type.</summary>

namespace Allors.Database.Domain.Tests
{
    using System.Linq;
    using Resources;
    using Xunit;

    public class StatementOfWorkRuleTests : DomainTest, IClassFixture<Fixture>
    {
        public StatementOfWorkRuleTests(Fixture fixture) : base(fixture) { }

        [Fact]
        public void ChangedIssuerThrowValidationError()
        {
            var quote = new StatementOfWorkBuilder(this.Transaction).Build();
            this.Derive();

            quote.Issuer = new OrganisationBuilder(this.Transaction).WithIsInternalOrganisation(true).Build();

            Assert.Contains(ErrorMessages.InternalOrganisationChanged, this.Derive().Errors.Select(v => v.Message));
        }
    }

    [Trait("Category", "Security")]
    public class StatementOfWorkDeniedPermissionRuleTests : DomainTest, IClassFixture<Fixture>
    {
        public StatementOfWorkDeniedPermissionRuleTests(Fixture fixture) : base(fixture) => this.deleteRevocation = new Revocations(this.Transaction).StatementOfWorkDeleteRevocation;

        public override Config Config => new Config { SetupSecurity = true };

        private readonly Revocation deleteRevocation;

        [Fact]
        public void OnChangedStatementOfWorkTransitionalDeniedPermissionsDeriveDeletePermissionAllowed()
        {
            var quote = new StatementOfWorkBuilder(this.Transaction).Build();
            this.Derive();

            Assert.DoesNotContain(this.deleteRevocation, quote.Revocations);
        }

        [Fact]
        public void OnChangedStatementOfWorkStateTransitionalDeniedPermissionsDeriveDeletePermissionDenied()
        {
            var quote = new StatementOfWorkBuilder(this.Transaction).Build();
            this.Derive();

            quote.QuoteState = new QuoteStates(this.Transaction).Accepted;
            this.Derive();

            Assert.Contains(this.deleteRevocation, quote.Revocations);
        }

        [Fact]
        public void OnChangedRequestDeriveDeletePermissionDenied()
        {
            var quote = new StatementOfWorkBuilder(this.Transaction).Build();
            this.Derive();

            quote.Request = new RequestForQuoteBuilder(this.Transaction).Build();
            this.Derive();

            Assert.Contains(this.deleteRevocation, quote.Revocations);
        }

        [Fact]
        public void OnChangedRequestDeriveDeletePermissionDeniedAllowed()
        {
            var quote = new StatementOfWorkBuilder(this.Transaction).Build();
            this.Derive();

            quote.Request = new RequestForQuoteBuilder(this.Transaction).Build();
            this.Derive();

            quote.RemoveRequest();
            this.Derive();

            Assert.DoesNotContain(this.deleteRevocation, quote.Revocations);
        }
    }
}
