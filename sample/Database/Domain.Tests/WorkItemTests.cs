// <copyright file="WorkItemTests.cs" company="Allors bv">
// Copyright (c) Allors bv. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Database.Domain.Tests
{
    using Xunit;

    public class WorkItemTests : DomainTest, IClassFixture<Fixture>
    {
        public WorkItemTests(Fixture fixture) : base(fixture) { }

        [Fact]
        public void TheRuleOfTheApplicationCountsTheOpenWorkItemsOfAPerson()
        {
            var jane = new PersonBuilder(this.Transaction).WithFirstName("Jane").WithLastName("Doe").Build();
            var john = new PersonBuilder(this.Transaction).WithFirstName("John").WithLastName("Doe").Build();

            var model = new WorkItemBuilder(this.Transaction).WithTitle("Declare the model").WithAssignee(jane).Build();
            var rule = new WorkItemBuilder(this.Transaction).WithTitle("Write the rule").WithAssignee(jane).Build();

            this.Transaction.Derive();

            Assert.Equal(2, jane.OpenWorkItemCount);
            Assert.Equal(0, john.OpenWorkItemCount);

            model.Done = true;

            this.Transaction.Derive();

            Assert.Equal(1, jane.OpenWorkItemCount);

            rule.Assignee = john;

            this.Transaction.Derive();

            Assert.Equal(0, jane.OpenWorkItemCount);
            Assert.Equal(1, john.OpenWorkItemCount);
        }

        [Fact]
        public void ARuleOfCoreRunsForAClassOfTheApplication()
        {
            var jane = new PersonBuilder(this.Transaction).WithUserName("jane@example.com").Build();

            this.Transaction.Derive();

            Assert.Equal("JANE@EXAMPLE.COM", jane.NormalizedUserName);
        }

        [Fact]
        public void ARequiredRoleWithoutAValueIsAnErrorWithAMessageOfCore()
        {
            new WorkItemBuilder(this.Transaction).Build();

            var validation = this.Transaction.Derive(false);

            var error = Assert.Single(validation.Errors);
            Assert.Equal("WorkItem.Title is required", error.Message);
        }

        [Fact]
        public void TheSetupOfCoreRunsThroughTheHooksOfTheApplication()
        {
            Assert.NotNull(new UserGroups(this.Transaction).Administrators);
            Assert.NotNull(new Roles(this.Transaction).Administrator);
            Assert.Empty(new People(this.Transaction).Extent());
        }
    }
}
