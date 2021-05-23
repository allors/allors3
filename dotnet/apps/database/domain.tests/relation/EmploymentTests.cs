// <copyright file="EmploymentTests.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>
// <summary>Defines the MediaTests type.</summary>

namespace Allors.Database.Domain.Tests
{
    using Xunit;

    public class EmploymentTests : DomainTest, IClassFixture<Fixture>
    {
        private Person employee;
        private InternalOrganisation internalOrganisation;
        private Employment employment;

        public EmploymentTests(Fixture fixture) : base(fixture)
        {
            this.employee = new PersonBuilder(this.Transaction).WithLastName("slave").Build();

            this.employment = new EmploymentBuilder(this.Transaction)
                .WithEmployee(this.employee)
                .WithFromDate(this.Transaction.Now())
                .Build();

            this.Transaction.Derive();
            this.Transaction.Commit();
        }

        [Fact]
        public void GivenActiveEmployment_WhenDeriving_ThenInternalOrganisationEmployeesContainsEmployee()
        {
            var employee = new PersonBuilder(this.Transaction).WithLastName("customer").Build();
            var employer = this.InternalOrganisation;

            new EmploymentBuilder(this.Transaction)
                .WithEmployee(employee)
                .WithEmployer(employer)
                .Build();

            this.Transaction.Derive();

            Assert.Contains(employee, employer.ActiveEmployees);
        }

        [Fact]
        public void GivenEmploymentToCome_WhenDeriving_ThenInternalOrganisationEmployeesDosNotContainEmployee()
        {
            var employee = new PersonBuilder(this.Transaction).WithLastName("customer").Build();
            var employer = this.InternalOrganisation;

            new EmploymentBuilder(this.Transaction)
                .WithEmployee(employee)
                .WithFromDate(this.Transaction.Now().AddDays(1))
                .Build();

            this.Transaction.Derive();

            Assert.DoesNotContain(employee, employer.ActiveEmployees);
        }

        [Fact]
        public void GivenEmploymentThatHasEnded_WhenDeriving_ThenInternalOrganisationEmployeesDosNotContainEmployee()
        {
            var employee = new PersonBuilder(this.Transaction).WithLastName("customer").Build();
            var employer = this.InternalOrganisation;

            new EmploymentBuilder(this.Transaction)
                .WithEmployee(employee)
                .WithFromDate(this.Transaction.Now().AddDays(-10))
                .WithThroughDate(this.Transaction.Now().AddDays(-1))
                .Build();

            this.Transaction.Derive();

            Assert.DoesNotContain(employee, employer.ActiveEmployees);
        }

        private void InstantiateObjects(ITransaction transaction)
        {
            this.employee = (Person)transaction.Instantiate(this.employee);
            this.internalOrganisation = (InternalOrganisation)transaction.Instantiate(this.internalOrganisation);
            this.employment = (Employment)transaction.Instantiate(this.employment);
        }
    }

    public class EmploymentRuleTests : DomainTest, IClassFixture<Fixture>
    {
        public EmploymentRuleTests(Fixture fixture) : base(fixture) { }

        [Fact]
        public void ChangedEmployeeDeriveParties()
        {
            var employment = new EmploymentBuilder(this.Transaction).Build();
            this.Transaction.Derive(false);

            var customer = new PersonBuilder(this.Transaction).Build();
            employment.Employee = customer;
            this.Transaction.Derive(false);

            Assert.Contains(customer, employment.Parties);
        }

        [Fact]
        public void ChangedEmployerDeriveParties()
        {
            var employment = new EmploymentBuilder(this.Transaction).Build();
            this.Transaction.Derive(false);

            var internalOrganisation = new OrganisationBuilder(this.Transaction).WithIsInternalOrganisation(true).Build();
            employment.Employer = internalOrganisation;
            this.Transaction.Derive(false);

            Assert.Contains(internalOrganisation, employment.Parties);
        }
    }
}
