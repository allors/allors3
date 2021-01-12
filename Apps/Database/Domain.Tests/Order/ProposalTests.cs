// <copyright file="ProposalDeniedPermissionDerivationTests.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>
// <summary>Defines the MediaTests type.</summary>

namespace Allors.Database.Domain.Tests
{
    using Resources;
    using Xunit;

    public class ProposalDerivationTests : DomainTest, IClassFixture<Fixture>
    {
        public ProposalDerivationTests(Fixture fixture) : base(fixture) { }

        [Fact]
        public void ChangedIssuerThrowValidationError()
        {
            var quote = new ProposalBuilder(this.Session).Build();
            this.Session.Derive(false);

            quote.Issuer = new OrganisationBuilder(this.Session).WithIsInternalOrganisation(true).Build();

            var expectedError = $"{quote} {this.M.ProductQuote.Issuer} {ErrorMessages.InternalOrganisationChanged}";
            Assert.Equal(expectedError, this.Session.Derive(false).Errors[0].Message);
        }
    }

    [Trait("Category", "Security")]
    public class ProposalDeniedPermissionDerivationTests : DomainTest, IClassFixture<Fixture>
    {
        public ProposalDeniedPermissionDerivationTests(Fixture fixture) : base(fixture) => this.deletePermission = new Permissions(this.Session).Get(this.M.Proposal.ObjectType, this.M.Proposal.Delete);

        public override Config Config => new Config { SetupSecurity = true };

        private readonly Permission deletePermission;


        [Fact]
        public void OnChangedProposalStateCreatedDeriveDeletePermission()
        {
            var proposal = new ProposalBuilder(this.Session).Build();

            this.Session.Derive(false);

            Assert.DoesNotContain(this.deletePermission, proposal.DeniedPermissions);
        }

        [Fact]
        public void OnChangedProposalStateZDeriveDeletePermission()
        {
            var proposal = new ProposalBuilder(this.Session).Build();
            this.Session.Derive(false);

            proposal.Accept();
            this.Session.Derive(false);

            Assert.Contains(this.deletePermission, proposal.DeniedPermissions);
        }
    }
}
