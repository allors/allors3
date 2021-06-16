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
            this.Transaction.Derive();

            var party = new PersonBuilder(this.Transaction).WithLastName("party").Build();

            var request = new RequestForQuoteBuilder(this.Transaction)
                .WithOriginator(party)
                .WithFullfillContactMechanism(new WebAddressBuilder(this.Transaction).WithElectronicAddressString("test").Build())
                .Build();

            this.Transaction.Derive();

            Assert.Equal(int.Parse(request.RequestNumber), request.SortableRequestNumber);
        }

        [Fact]
        public void GivenIssuerWithRequestNumberPrefix_WhenDeriving_ThenSortableRequestNumberIsSet()
        {
            this.InternalOrganisation.RequestSequence = new RequestSequences(this.Transaction).EnforcedSequence;
            this.InternalOrganisation.RequestNumberPrefix = "prefix-";
            this.Transaction.Derive();

            var party = new PersonBuilder(this.Transaction).WithLastName("party").Build();

            var request = new RequestForQuoteBuilder(this.Transaction)
                .WithOriginator(party)
                .WithFullfillContactMechanism(new WebAddressBuilder(this.Transaction).WithElectronicAddressString("test").Build())
                .Build();

            this.Transaction.Derive();

            Assert.Equal(int.Parse(request.RequestNumber.Split('-')[1]), request.SortableRequestNumber);
        }

        [Fact]
        public void GivenIssuerWithParametrizedRequestNumberPrefix_WhenDeriving_ThenSortableRequestNumberIsSet()
        {
            this.InternalOrganisation.RequestSequence = new RequestSequences(this.Transaction).EnforcedSequence;
            this.InternalOrganisation.RequestNumberPrefix = "prefix-{year}-";
            this.Transaction.Derive();

            var party = new PersonBuilder(this.Transaction).WithLastName("party").Build();

            var request = new RequestForQuoteBuilder(this.Transaction)
                .WithOriginator(party)
                .WithFullfillContactMechanism(new WebAddressBuilder(this.Transaction).WithElectronicAddressString("test").Build())
                .WithRequestDate(this.Transaction.Now().Date)
                .Build();

            this.Transaction.Derive();

            var number = int.Parse(request.RequestNumber.Split('-').Last()).ToString("000000");
            Assert.Equal(int.Parse(string.Concat(this.Transaction.Now().Date.Year.ToString(), number)), request.SortableRequestNumber);
        }
    }

    public class RequestAnonymousRuleTests : DomainTest, IClassFixture<Fixture>
    {
        public RequestAnonymousRuleTests(Fixture fixture) : base(fixture) { }

        [Fact]
        public void ChangedOriginatorDeriveRequestState()
        {
            var request = new RequestForQuoteBuilder(this.Transaction).Build();
            this.Derive();

            request.Originator = this.InternalOrganisation.ActiveCustomers.First;
            this.Derive();

            Assert.True(request.RequestState.IsSubmitted);
        }

        [Fact]
        public void ChangedOriginatorAddPartyContactMechanismEmailAddress()
        {
            var request = new RequestForQuoteBuilder(this.Transaction)
                .WithEmailAddress("emailaddress")
                .Build();
            this.Derive();

            var customer = this.InternalOrganisation.ActiveCustomers.First;
            request.Originator = customer;
            this.Derive();

            Assert.NotNull(customer.PartyContactMechanisms.Where(v => v.ContactMechanism.GetType().Name == typeof(EmailAddress).Name).FirstOrDefault(v => ((EmailAddress)v.ContactMechanism).ElectronicAddressString.Equals("emailaddress")));
        }

        [Fact]
        public void ChangedOriginatorAddPartyContactMechanismTelecommunicationsNumber()
        {
            var request = new RequestForQuoteBuilder(this.Transaction)
                .WithTelephoneNumber("phone")
                .Build();
            this.Derive();

            var customer = this.InternalOrganisation.ActiveCustomers.First;
            request.Originator = customer;
            this.Derive();

            Assert.NotNull(customer.PartyContactMechanisms.Where(v => v.ContactMechanism.GetType().Name == typeof(TelecommunicationsNumber).Name).FirstOrDefault(v => ((TelecommunicationsNumber)v.ContactMechanism).ContactNumber.Equals("phone")));
        }
    }

    public class RequestRuleTests : DomainTest, IClassFixture<Fixture>
    {
        public RequestRuleTests(Fixture fixture) : base(fixture) { }

        [Fact]
        public void ChangedAssignedCurrencyDeriveDerivedCurrency()
        {
            var request = new RequestForQuoteBuilder(this.Transaction).Build();
            this.Derive();

            request.AssignedCurrency = new Currencies(this.Transaction).FindBy(M.Currency.IsoCode, "SEK");
            this.Derive();

            Assert.Equal(request.DerivedCurrency, request.AssignedCurrency);
        }

        [Fact]
        public void ChangedRecipientDeriveDerivedCurrency()
        {
            Assert.True(this.InternalOrganisation.ExistPreferredCurrency);

            var request = new RequestForQuoteBuilder(this.Transaction).Build();
            this.Derive();

            Assert.Equal(request.DerivedCurrency, this.InternalOrganisation.PreferredCurrency);
        }

        [Fact]
        public void ChangedRecipientPreferredCurrencyDeriveDerivedCurrency()
        {
            var request = new RequestForQuoteBuilder(this.Transaction).Build();
            this.Derive();

            var swedishKrona = new Currencies(this.Transaction).FindBy(M.Currency.IsoCode, "SEK");
            this.InternalOrganisation.PreferredCurrency = swedishKrona;
            this.Derive();

            Assert.Equal(request.DerivedCurrency, swedishKrona);
        }

        [Fact]
        public void ChangedOriginatorDeriveDerivedCurrency()
        {
            var swedishKrona = new Currencies(this.Transaction).FindBy(M.Currency.IsoCode, "SEK");
            var customer = this.InternalOrganisation.ActiveCustomers.First;
            customer.PreferredCurrency = swedishKrona;
            this.Derive();

            var request = new RequestForQuoteBuilder(this.Transaction).Build();
            this.Derive();

            request.Originator = customer;
            this.Derive();

            Assert.Equal(request.DerivedCurrency, swedishKrona);
        }

        [Fact]
        public void ChangedOriginatorPreferredCurrencyDeriveDerivedCurrency()
        {
            var customer = this.InternalOrganisation.ActiveCustomers.First;

            var request = new RequestForQuoteBuilder(this.Transaction).WithOriginator(customer).Build();
            this.Derive();

            var swedishKrona = new Currencies(this.Transaction).FindBy(M.Currency.IsoCode, "SEK");
            customer.PreferredCurrency = swedishKrona;
            this.Derive();

            Assert.Equal(request.DerivedCurrency, swedishKrona);
        }

        [Fact]
        public void ChangedStoreDeriveOrderNumber()
        {
            var request = new RequestForQuoteBuilder(this.Transaction).Build();
            this.InternalOrganisation.RemoveRequestNumberPrefix();
            var number = this.InternalOrganisation.RequestNumberCounter.Value;

            this.Derive();

            Assert.Equal(request.RequestNumber, (number + 1).ToString());
        }

        [Fact]
        public void ChangedStoreDeriveSortableOrderNumber()
        {
            var request = new RequestForQuoteBuilder(this.Transaction).Build();
            var number = this.InternalOrganisation.RequestNumberCounter.Value;

            this.Derive();

            Assert.Equal(request.SortableRequestNumber.Value, number + 1);
        }
    }

    [Trait("Category", "Security")]
    public class RequestDeniedPermissonDerivationSecurityTests : DomainTest, IClassFixture<Fixture>
    {
        public RequestDeniedPermissonDerivationSecurityTests(Fixture fixture) : base(fixture) => this.deletePermission = new Permissions(this.Transaction).Get(this.M.RequestForInformation, this.M.RequestForInformation.Delete);

        public override Config Config => new Config { SetupSecurity = true };

        private readonly Permission deletePermission;

        [Fact]
        public void OnChangedRequestStateSubmittedDeriveDeletePermission()
        {
            var requestForInformation = new RequestForInformationBuilder(this.Transaction)
                .WithOriginator(this.InternalOrganisation).Build();
            this.Derive();

            Assert.DoesNotContain(this.deletePermission, requestForInformation.DeniedPermissions);
        }

        [Fact]
        public void OnChangedRequestStateAnonymousDeriveDeletePermission()
        {
            var requestForInformation = new RequestForInformationBuilder(this.Transaction).WithEmailAddress("test@test.com").Build();
            this.Derive();

            Assert.Contains(this.deletePermission, requestForInformation.DeniedPermissions);
        }

        [Fact]
        public void OnChangedRequestStateSubmittedWithQuoteDeriveDeletePermission()
        {
            var requestForInformation = new RequestForInformationBuilder(this.Transaction)
                .WithOriginator(this.InternalOrganisation)
                .Build();
            this.Derive();

            var quote = new ProductQuoteBuilder(this.Transaction).WithRequest(requestForInformation).Build();
            this.Derive();

            Assert.Contains(this.deletePermission, requestForInformation.DeniedPermissions);
        }
    }
}
