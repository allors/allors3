// <copyright file="OrderTermTests.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>
// <summary>Defines the MediaTests type.</summary>

namespace Allors.Database.Domain.Tests
{
    using Resources;
    using Xunit;

    public class StatementOfWorkRuleTests : DomainTest, IClassFixture<Fixture>
    {
        public StatementOfWorkRuleTests(Fixture fixture) : base(fixture) { }

        [Fact]
        public void ChangedIssuerThrowValidationError()
        {
            var quote = new StatementOfWorkBuilder(this.Transaction).Build();
            this.Transaction.Derive(false);

            quote.Issuer = new OrganisationBuilder(this.Transaction).WithIsInternalOrganisation(true).Build();

            var expectedError = $"{quote} {this.M.ProductQuote.Issuer} {ErrorMessages.InternalOrganisationChanged}";
            Assert.Equal(expectedError, this.Transaction.Derive(false).Errors[0].Message);
        }
    }

    [Trait("Category", "Security")]
    public class StatementOfWorkDeniedPermissionRuleTests : DomainTest, IClassFixture<Fixture>
    {
        public StatementOfWorkDeniedPermissionRuleTests(Fixture fixture) : base(fixture) => this.deletePermission = new Permissions(this.Transaction).Get(this.M.StatementOfWork, this.M.StatementOfWork.Delete);

        public override Config Config => new Config { SetupSecurity = true };

        private readonly Permission deletePermission;

        [Fact]
        public void OnChangedStatementOfWorkTransitionalDeniedPermissionsDeriveDeletePermissionAllowed()
        {
            var quote = new StatementOfWorkBuilder(this.Transaction).Build();
            this.Transaction.Derive(false);

            Assert.DoesNotContain(this.deletePermission, quote.DeniedPermissions);
        }

        [Fact]
        public void OnChangedStatementOfWorkStateTransitionalDeniedPermissionsDeriveDeletePermissionDenied()
        {
            var quote = new StatementOfWorkBuilder(this.Transaction).Build();
            this.Transaction.Derive(false);

            quote.QuoteState = new QuoteStates(this.Transaction).Accepted;
            this.Transaction.Derive(false);

            Assert.Contains(this.deletePermission, quote.DeniedPermissions);
        }

        [Fact]
        public void OnChangedRequestDeriveDeletePermissionDenied()
        {
            var quote = new StatementOfWorkBuilder(this.Transaction).Build();
            this.Transaction.Derive(false);

            quote.Request = new RequestForQuoteBuilder(this.Transaction).Build();
            this.Transaction.Derive(false);

            Assert.Contains(this.deletePermission, quote.DeniedPermissions);
        }

        [Fact]
        public void OnChangedRequestDeriveDeletePermissionDeniedAllowed()
        {
            var quote = new StatementOfWorkBuilder(this.Transaction).Build();
            this.Transaction.Derive(false);

            quote.Request = new RequestForQuoteBuilder(this.Transaction).Build();
            this.Transaction.Derive(false);

            quote.RemoveRequest();
            this.Transaction.Derive(false);

            Assert.DoesNotContain(this.deletePermission, quote.DeniedPermissions);
        }
    }
}
