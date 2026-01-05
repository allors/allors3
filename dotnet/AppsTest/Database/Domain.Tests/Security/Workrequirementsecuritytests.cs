// <copyright file="WorkEffortSecurityTests.cs" company="Allors bv">
// Copyright (c) Allors bv. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Database.Domain.Tests
{
    using System.Linq;
    using Xunit;
    using TestPopulation;

    [Trait("Category", "Security")]
    public class WorkRequirementSecurityTests : DomainTest, IClassFixture<Fixture>
    {
        public WorkRequirementSecurityTests(Fixture fixture) : base(fixture) { }

        public override Config Config => new Config { SetupSecurity = true };

        [Fact]
        public void WorkRequirement_StateCreated()
        {
            var customer = new OrganisationBuilder(this.Transaction).WithName("Org1").Build();
            var internalOrganisation = new Organisations(this.Transaction).Extent().First(o => o.IsInternalOrganisation);
            new CustomerRelationshipBuilder(this.Transaction).WithCustomer(customer).WithInternalOrganisation(internalOrganisation).Build();

            var serialisedItem = new SerialisedItemBuilder(this.Transaction).WithDefaults(internalOrganisation).Build();
            serialisedItem.OwnedBy = customer;

            var workRequirement = new WorkRequirementBuilder(this.Transaction).WithDescription("desc").WithFixedAsset(serialisedItem).Build();

            this.Transaction.Derive();

            Assert.Equal(new RequirementStates(this.Transaction).Created, workRequirement.RequirementState);

            User user = this.Administrator;
            this.Transaction.SetUser(user);

            var acl = new DatabaseAccessControl(this.Security, this.Administrator)[workRequirement];
            Assert.True(acl.CanExecute(this.M.WorkRequirement.Delete));
            Assert.True(acl.CanExecute(this.M.WorkRequirement.Cancel));
            Assert.False(acl.CanExecute(this.M.WorkRequirement.Reopen));
            Assert.True(acl.CanExecute(this.M.WorkRequirement.Start));
            Assert.False(acl.CanExecute(this.M.WorkRequirement.Close));
            Assert.True(acl.CanExecute(this.M.WorkRequirement.CreateWorkTask));
        }

        [Fact]
        public void WorkRequirement_StateCancelled()
        {
            var customer = new OrganisationBuilder(this.Transaction).WithName("Org1").Build();
            var internalOrganisation = new Organisations(this.Transaction).Extent().First(o => o.IsInternalOrganisation);
            new CustomerRelationshipBuilder(this.Transaction).WithCustomer(customer).WithInternalOrganisation(internalOrganisation).Build();

            var serialisedItem = new SerialisedItemBuilder(this.Transaction).WithDefaults(internalOrganisation).Build();
            serialisedItem.OwnedBy = customer;

            var workRequirement = new WorkRequirementBuilder(this.Transaction).WithDescription("desc").WithFixedAsset(serialisedItem).Build();

            this.Transaction.Derive();

            workRequirement.Cancel();

            this.Transaction.Derive();

            Assert.Equal(new RequirementStates(this.Transaction).Cancelled, workRequirement.RequirementState);

            User user = this.Administrator;
            this.Transaction.SetUser(user);

            var acl = new DatabaseAccessControl(this.Security, this.Administrator)[workRequirement];
            Assert.True(acl.CanExecute(this.M.WorkRequirement.Delete));
            Assert.False(acl.CanExecute(this.M.WorkRequirement.Cancel));
            Assert.True(acl.CanExecute(this.M.WorkRequirement.Reopen));
            Assert.False(acl.CanExecute(this.M.WorkRequirement.Close));
            Assert.False(acl.CanExecute(this.M.WorkRequirement.CreateWorkTask));
        }

        [Fact]
        public void WorkRequirement_StateClosed()
        {
            var customer = new OrganisationBuilder(this.Transaction).WithName("Org1").Build();
            var internalOrganisation = new Organisations(this.Transaction).Extent().First(o => o.IsInternalOrganisation);
            new CustomerRelationshipBuilder(this.Transaction).WithCustomer(customer).WithInternalOrganisation(internalOrganisation).Build();

            var serialisedItem = new SerialisedItemBuilder(this.Transaction).WithDefaults(internalOrganisation).Build();
            serialisedItem.OwnedBy = customer;

            var workRequirement = new WorkRequirementBuilder(this.Transaction).WithDescription("desc").WithFixedAsset(serialisedItem).Build();

            this.Transaction.Derive();

            workRequirement.Start();
            this.Transaction.Derive();

            workRequirement.Close();
            this.Transaction.Derive();

            Assert.Equal(new RequirementStates(this.Transaction).Finished, workRequirement.RequirementState);

            User user = this.Administrator;
            this.Transaction.SetUser(user);

            var acl = new DatabaseAccessControl(this.Security, this.Administrator)[workRequirement];
            Assert.False(acl.CanExecute(this.M.WorkRequirement.Delete));
            Assert.False(acl.CanExecute(this.M.WorkRequirement.Cancel));
            Assert.True(acl.CanExecute(this.M.WorkRequirement.Reopen));
            Assert.False(acl.CanExecute(this.M.WorkRequirement.Close));
            Assert.False(acl.CanExecute(this.M.WorkRequirement.CreateWorkTask));
        }

        [Fact]
        public void WorkRequirement_StateInProgress()
        {
            var customer = new OrganisationBuilder(this.Transaction).WithName("Org1").Build();
            var internalOrganisation = new Organisations(this.Transaction).Extent().First(o => o.IsInternalOrganisation);
            new CustomerRelationshipBuilder(this.Transaction).WithCustomer(customer).WithInternalOrganisation(internalOrganisation).Build();

            var serialisedItem = new SerialisedItemBuilder(this.Transaction).WithDefaults(internalOrganisation).Build();
            serialisedItem.OwnedBy = customer;

            var workRequirement = new WorkRequirementBuilder(this.Transaction).WithDescription("desc").WithFixedAsset(serialisedItem).Build();

            this.Transaction.Derive();

            workRequirement.CreateWorkTask();

            this.Transaction.Derive(false);

            Assert.Equal(new RequirementStates(this.Transaction).InProgress, workRequirement.RequirementState);

            User user = this.Administrator;
            this.Transaction.SetUser(user);

            var acl = new DatabaseAccessControl(this.Security, this.Administrator)[workRequirement];
            Assert.False(acl.CanExecute(this.M.WorkRequirement.Delete));
            Assert.False(acl.CanExecute(this.M.WorkRequirement.Cancel));
            Assert.False(acl.CanExecute(this.M.WorkRequirement.Reopen));
            Assert.True(acl.CanExecute(this.M.WorkRequirement.Close));
            Assert.False(acl.CanExecute(this.M.WorkRequirement.CreateWorkTask));
        }
    }

    [Trait("Category", "Security")]
    public class WorkRequirementDeniedPermissionRuleTests : DomainTest, IClassFixture<Fixture>
    {
        public WorkRequirementDeniedPermissionRuleTests(Fixture fixture) : base(fixture)
        {
            this.cancelRevocation = new Revocations(this.Transaction).WorkRequirementCancelRevocation;
            this.reopenRevocation = new Revocations(this.Transaction).WorkRequirementReopenRevocation;
        }

        public override Config Config => new Config { SetupSecurity = true };

        private readonly Revocation cancelRevocation;
        private readonly Revocation reopenRevocation;

        [Fact]
        public void OnChangedWorkTaskDeriveCancelAndReopenPermission()
        {
            var customer = new OrganisationBuilder(this.Transaction).WithName("Org1").Build();
            var internalOrganisation = new Organisations(this.Transaction).Extent().First(o => o.IsInternalOrganisation);
            new CustomerRelationshipBuilder(this.Transaction).WithCustomer(customer).WithInternalOrganisation(internalOrganisation).Build();

            var serialisedItem = new SerialisedItemBuilder(this.Transaction).WithDefaults(internalOrganisation).Build();
            serialisedItem.OwnedBy = customer;

            var workRequirement = new WorkRequirementBuilder(this.Transaction).WithDescription("desc").WithFixedAsset(serialisedItem).Build();

            this.Transaction.Derive();

            workRequirement.CreateWorkTask();

            this.Transaction.Derive(false);

            Assert.Contains(this.cancelRevocation, workRequirement.Revocations);
            Assert.Contains(this.reopenRevocation, workRequirement.Revocations);

            User user = this.Administrator;
            this.Transaction.SetUser(user);

            var acl = new DatabaseAccessControl(this.Security, this.Administrator)[workRequirement];
            Assert.False(acl.CanExecute(this.M.WorkRequirement.Cancel));
            Assert.False(acl.CanExecute(this.M.WorkRequirement.Reopen));
        }
    }
}
