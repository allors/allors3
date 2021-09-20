// <copyright file="EmployeeTests.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Database.Domain.Tests
{
    using System.Linq;
    using Xunit;

    [Trait("Category", "Security")]
    public class EmployeeSecurityTests : DomainTest, IClassFixture<Fixture>
    {
        public EmployeeSecurityTests(Fixture fixture) : base(fixture) { }

        public override Config Config => new Config { SetupSecurity = true };

        [Fact]
        public void Person()
        {
            var employee = new Employments(this.Transaction).Extent().Select(v => v.Employee).First();
            this.Transaction.SetUser(employee);

            var acl = new DatabaseAccessControlLists(employee)[employee];
            Assert.True(acl.CanRead(this.M.Person.FirstName));
            Assert.False(acl.CanWrite(this.M.Person.FirstName));
        }

        [Fact]
        public void Good()
        {
            var good = new Goods(this.Transaction).Extent().First();

            var employee = new Employments(this.Transaction).Extent().Select(v => v.Employee).First();
            this.Transaction.SetUser(employee);

            var acl = new DatabaseAccessControlLists(employee)[good];
            Assert.True(acl.CanRead(this.M.Good.Name));
            Assert.False(acl.CanWrite(this.M.Good.Name));
        }

        [Fact]
        public void WorkTaskNewInTransaction()
        {
            var customer = new OrganisationBuilder(this.Transaction).WithName("Org1").Build();
            var internalOrganisation = new Organisations(this.Transaction).Extent().First(o => o.IsInternalOrganisation);
            new CustomerRelationshipBuilder(this.Transaction).WithCustomer(customer).WithInternalOrganisation(internalOrganisation).Build();

            var workTask = new WorkTaskBuilder(this.Transaction).WithName("worktask").WithCustomer(customer).Build();

            this.Transaction.Derive();

            var employee = new Employments(this.Transaction).Extent().Select(v => v.Employee).First();
            this.Transaction.SetUser(employee);

            Assert.True(workTask.Strategy.IsNewInTransaction);

            var acl = new DatabaseAccessControlLists(employee)[workTask];
            Assert.True(acl.CanRead(this.M.WorkTask.Name));
            Assert.True(acl.CanWrite(this.M.WorkTask.Name));
        }

        [Fact]
        public void WorkTask()
        {
            var customer = new OrganisationBuilder(this.Transaction).WithName("Org1").Build();
            var internalOrganisation = new Organisations(this.Transaction).Extent().First(o => o.IsInternalOrganisation);
            new CustomerRelationshipBuilder(this.Transaction).WithCustomer(customer).WithInternalOrganisation(internalOrganisation).Build();

            var workTask = new WorkTaskBuilder(this.Transaction).WithName("worktask").WithCustomer(customer).Build();

            this.Transaction.Derive();
            this.Transaction.Commit();

            var employee = new Employments(this.Transaction).Extent().Select(v => v.Employee).First();
            this.Transaction.SetUser(employee);

            Assert.False(workTask.Strategy.IsNewInTransaction);

            var acl = new DatabaseAccessControlLists(employee)[workTask];
            Assert.True(acl.CanRead(this.M.WorkTask.Name));
            Assert.False(acl.CanWrite(this.M.WorkTask.Name));
        }

        [Fact]
        public void SalesInvoice()
        {
            var customer = new OrganisationBuilder(this.Transaction).WithName("Org1").Build();
            var contactMechanism = new PostalAddressBuilder(this.Transaction)
                .WithAddress1("Haverwerf 15")
                .WithLocality("Mechelen")
                .WithCountry(new Countries(this.Transaction).FindBy(this.M.Country.IsoCode, "BE"))
                .Build();

            var internalOrganisation = new Organisations(this.Transaction).Extent().First(o => o.IsInternalOrganisation);
            new CustomerRelationshipBuilder(this.Transaction).WithCustomer(customer).WithInternalOrganisation(internalOrganisation).Build();

            var salesInvoice = new SalesInvoiceBuilder(this.Transaction).WithBillToCustomer(customer).WithAssignedBillToContactMechanism(contactMechanism).Build();

            this.Transaction.Derive();

            var employee = new Employments(this.Transaction).Extent().Select(v => v.Employee).First();
            this.Transaction.SetUser(employee);

            Assert.True(salesInvoice.Strategy.IsNewInTransaction);

            var acl = new DatabaseAccessControlLists(employee)[salesInvoice];
            Assert.True(acl.CanRead(this.M.SalesInvoice.Description));
            Assert.True(acl.CanWrite(this.M.SalesInvoice.Description));

            this.Transaction.Commit();

            Assert.False(salesInvoice.Strategy.IsNewInTransaction);

            acl = new DatabaseAccessControlLists(employee)[salesInvoice];
            Assert.True(acl.CanRead(this.M.SalesInvoice.Description));
            Assert.False(acl.CanWrite(this.M.SalesInvoice.Description));
        }

        [Fact]
        public void SalesOrder()
        {
            var customer = new PersonBuilder(this.Transaction).WithFirstName("Koen").WithUserName("customer").Build();

            new CustomerRelationshipBuilder(this.Transaction).WithFromDate(this.Transaction.Now()).WithCustomer(customer).WithInternalOrganisation(this.InternalOrganisation).Build();

            var mechelen = new CityBuilder(this.Transaction).WithName("Mechelen").Build();

            this.Transaction.Derive();

            var employee = new Employments(this.Transaction).Extent().Select(v => v.Employee).First();
            this.Transaction.SetUser(employee);

            var order = new SalesOrderBuilder(this.Transaction)
                .WithTakenBy(this.InternalOrganisation)
                .WithBillToCustomer(customer)
                .WithShipToCustomer(customer)
                .WithAssignedShipToAddress(new PostalAddressBuilder(this.Transaction).WithPostalAddressBoundary(mechelen).WithAddress1("Haverwerf 15").Build())
                .Build();

            this.Transaction.Derive();

            Assert.Equal(new SalesOrderStates(this.Transaction).Provisional, order.SalesOrderState);

            var acl = new DatabaseAccessControlLists(employee)[order];
            Assert.False(acl.CanExecute(this.M.SalesOrder.Ship));
            Assert.True(acl.CanWrite(this.M.SalesOrder.Description));
            Assert.True(acl.CanRead(this.M.SalesOrder.Description));
        }

        [Fact]
        public void UserGroup()
        {
            var userGroup = new UserGroups(this.Transaction).Administrators;

            var employee = new Employments(this.Transaction).Extent().Select(v => v.Employee).First();
            this.Transaction.SetUser(employee);

            var acl = new DatabaseAccessControlLists(employee)[userGroup];
            Assert.True(acl.CanRead(this.M.UserGroup.Members));
            Assert.False(acl.CanWrite(this.M.UserGroup.Members));
        }
    }
}
