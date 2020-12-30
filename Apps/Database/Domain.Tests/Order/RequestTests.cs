// <copyright file="RequestTests.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>
// <summary>Defines the MediaTests type.</summary>

namespace Allors.Database.Domain.Tests
{
    using System.Linq;
    using Xunit;

    public class RequestTests : DomainTest, IClassFixture<Fixture>
    {
        public RequestTests(Fixture fixture) : base(fixture) { }

        [Fact]
        public void GivenIssuerWithoutRequestNumberPrefix_WhenDeriving_ThenSortableRequestNumberIsSet()
        {
            this.InternalOrganisation.RemoveRequestNumberPrefix();
            this.Session.Derive();

            var party = new PersonBuilder(this.Session).WithLastName("party").Build();

            var request = new RequestForQuoteBuilder(this.Session)
                .WithOriginator(party)
                .WithFullfillContactMechanism(new WebAddressBuilder(this.Session).WithElectronicAddressString("test").Build())
                .Build();

            this.Session.Derive();

            Assert.Equal(int.Parse(request.RequestNumber), request.SortableRequestNumber);
        }

        [Fact]
        public void GivenIssuerWithRequestNumberPrefix_WhenDeriving_ThenSortableRequestNumberIsSet()
        {
            this.InternalOrganisation.InvoiceSequence = new InvoiceSequences(this.Session).EnforcedSequence;
            this.InternalOrganisation.RequestNumberPrefix = "prefix-";
            this.Session.Derive();

            var party = new PersonBuilder(this.Session).WithLastName("party").Build();

            var request = new RequestForQuoteBuilder(this.Session)
                .WithOriginator(party)
                .WithFullfillContactMechanism(new WebAddressBuilder(this.Session).WithElectronicAddressString("test").Build())
                .Build();

            this.Session.Derive();

            Assert.Equal(int.Parse(request.RequestNumber.Split('-')[1]), request.SortableRequestNumber);
        }

        [Fact]
        public void GivenIssuerWithParametrizedRequestNumberPrefix_WhenDeriving_ThenSortableRequestNumberIsSet()
        {
            this.InternalOrganisation.InvoiceSequence = new InvoiceSequences(this.Session).EnforcedSequence;
            this.InternalOrganisation.RequestNumberPrefix = "prefix-{year}-";
            this.Session.Derive();

            var party = new PersonBuilder(this.Session).WithLastName("party").Build();

            var request = new RequestForQuoteBuilder(this.Session)
                .WithOriginator(party)
                .WithFullfillContactMechanism(new WebAddressBuilder(this.Session).WithElectronicAddressString("test").Build())
                .WithRequestDate(this.Session.Now().Date)
                .Build();

            this.Session.Derive();

            Assert.Equal(int.Parse(string.Concat(this.Session.Now().Date.Year.ToString(), request.RequestNumber.Split('-').Last())), request.SortableRequestNumber);
        }
    }

    [Trait("Category", "Security")]
    public class RequestDeniedPermissonDerivationSecurityTests : DomainTest, IClassFixture<Fixture>
    {
        public RequestDeniedPermissonDerivationSecurityTests(Fixture fixture) : base(fixture) => this.deletePermission = new Permissions(this.Session).Get(this.M.RequestForInformation.ObjectType, this.M.RequestForInformation.Delete);

        public override Config Config => new Config { SetupSecurity = true };

        private readonly Permission deletePermission;

        [Fact]
        public void OnChangedRequestStateSubmittedDeriveDeletePermission()
        {
            var requestForInformation = new RequestForInformationBuilder(this.Session)
                .WithOriginator(this.InternalOrganisation).Build();
            this.Session.Derive(false);

            Assert.DoesNotContain(this.deletePermission, requestForInformation.DeniedPermissions);
        }

        [Fact]
        public void OnChangedRequestStateAnonymousDeriveDeletePermission()
        {
            var requestForInformation = new RequestForInformationBuilder(this.Session).WithEmailAddress("test@test.com").Build();
            this.Session.Derive(false);

            Assert.Contains(this.deletePermission, requestForInformation.DeniedPermissions);
        }

        [Fact]
        public void OnChangedRequestStateSubmittedWithQuoteDeriveDeletePermission()
        {
            var requestForInformation = new RequestForInformationBuilder(this.Session)
                .WithOriginator(this.InternalOrganisation)
                .Build();
            this.Session.Derive(false);

            var quote = new ProductQuoteBuilder(this.Session).WithRequest(requestForInformation).Build();
            this.Session.Derive(false);

            Assert.Contains(this.deletePermission, requestForInformation.DeniedPermissions);
        }
    }
}
