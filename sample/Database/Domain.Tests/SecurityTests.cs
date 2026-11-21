// <copyright file="SecurityTests.cs" company="Allors bv">
// Copyright (c) Allors bv. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Database.Domain.Tests
{
    using Xunit;

    public class SecurityTests : DomainTest, IClassFixture<Fixture>
    {
        public SecurityTests(Fixture fixture) : base(fixture) { }

        [Fact]
        public void AnAdministratorCanReadAndWriteEveryRoleThatIsNotDerived()
        {
            var jane = new PersonBuilder(this.Transaction).WithUserName("jane@example.com").Build();
            new UserGroups(this.Transaction).Administrators.AddMember(jane);
            var workItem = new WorkItemBuilder(this.Transaction).WithTitle("Write the rule").WithAssignee(jane).Build();
            this.Transaction.Derive();
            this.Transaction.Commit();

            var onTheWorkItem = this.AccessControl(jane)[workItem];
            Assert.True(onTheWorkItem.CanRead(this.M.WorkItem.Title));
            Assert.True(onTheWorkItem.CanWrite(this.M.WorkItem.Title));

            var onThePerson = this.AccessControl(jane)[jane];
            Assert.True(onThePerson.CanRead(this.M.Person.OpenWorkItemCount));
            Assert.False(onThePerson.CanWrite(this.M.Person.OpenWorkItemCount));
        }

        [Fact]
        public void AWorkerCanReadAWorkItemAndMarkItDone()
        {
            var john = new PersonBuilder(this.Transaction).WithUserName("john@example.com").Build();
            new UserGroups(this.Transaction).Workers.AddMember(john);
            var workItem = new WorkItemBuilder(this.Transaction).WithTitle("Write the rule").WithAssignee(john).Build();
            this.Transaction.Derive();
            this.Transaction.Commit();

            var onTheWorkItem = this.AccessControl(john)[workItem];
            Assert.True(onTheWorkItem.CanRead(this.M.WorkItem.Title));
            Assert.True(onTheWorkItem.CanWrite(this.M.WorkItem.Done));
            Assert.False(onTheWorkItem.CanWrite(this.M.WorkItem.Title));
            Assert.False(onTheWorkItem.CanWrite(this.M.WorkItem.Assignee));
        }

        [Fact]
        public void AWorkerCanReadTheNameOfAPersonAndNotTheUserName()
        {
            var john = new PersonBuilder(this.Transaction).WithUserName("john@example.com").Build();
            new UserGroups(this.Transaction).Workers.AddMember(john);
            var jane = new PersonBuilder(this.Transaction).WithFirstName("Jane").WithLastName("Doe").WithUserName("jane@example.com").Build();
            this.Transaction.Derive();
            this.Transaction.Commit();

            var onJane = this.AccessControl(john)[jane];
            Assert.True(onJane.CanRead(this.M.Person.FirstName));
            Assert.True(onJane.CanRead(this.M.Person.LastName));
            Assert.False(onJane.CanRead(this.M.Person.UserName));
            Assert.False(onJane.CanWrite(this.M.Person.FirstName));
        }

        [Fact]
        public void AUserInNoUserGroupCannotReadAWorkItem()
        {
            var john = new PersonBuilder(this.Transaction).WithUserName("john@example.com").Build();
            var workItem = new WorkItemBuilder(this.Transaction).WithTitle("Write the rule").WithAssignee(john).Build();
            this.Transaction.Derive();
            this.Transaction.Commit();

            Assert.False(this.AccessControl(john)[workItem].CanRead(this.M.WorkItem.Title));
        }

        private DatabaseAccessControl AccessControl(User user) => new DatabaseAccessControl(this.Transaction.Database.Services.Get<ISecurity>(), user);
    }
}
