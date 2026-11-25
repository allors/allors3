// <copyright file="WorkItemTests.cs" company="Allors bv">
// Copyright (c) Allors bv. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Workspace.Tests
{
    using System;
    using System.Threading.Tasks;
    using Allors.Workspace;
    using Allors.Workspace.Data;
    using Allors.Workspace.Domain;
    using Xunit;
    using DatabaseWorkItem = Allors.Database.Domain.WorkItem;
    using DatabaseWorkItems = Allors.Database.Domain.WorkItems;
    using M = Allors.Workspace.Meta.M;

    // People work with the work items through a workspace, as the security setup of the sample allows. The class
    // of each adapter derives from this one, so every test runs on Local and on Remote.
    public abstract class WorkItemTests : IDisposable
    {
        protected WorkItemTests(IProfile profile) => this.Profile = profile;

        public IProfile Profile { get; }

        public void Dispose() => this.Profile.Dispose();

        [Fact]
        public async Task AWorkerPullsTheWorkItemsWithTheirAssignees()
        {
            var workspace = await this.Profile.CreateWorkspace("jane@example.com");
            var m = workspace.Services.Get<M>();
            var session = workspace.CreateSession();

            var result = await session.PullAsync(new Pull
            {
                Extent = new Filter(m.WorkItem),
                Results = new[] { new Result { Include = m.WorkItem.Nodes(v => v.Assignee.Node()) } },
            });

            var workItem = Assert.Single(result.GetCollection<WorkItem>());
            Assert.Equal("Write the rule", workItem.Title);
            Assert.Equal("Jane", workItem.Assignee.FirstName);

            // A worker reads the names of people, and no other role of a person.
            Assert.False(workItem.Assignee.CanReadUserName);
            Assert.Null(workItem.Assignee.UserName);
        }

        [Fact]
        public async Task AWorkerMarksAWorkItemDone()
        {
            var workspace = await this.Profile.CreateWorkspace("jane@example.com");
            var m = workspace.Services.Get<M>();
            var session = workspace.CreateSession();

            var result = await session.PullAsync(new Pull { Extent = new Filter(m.WorkItem) });
            var workItem = Assert.Single(result.GetCollection<WorkItem>());

            // A worker marks work items done, and changes no other role of a work item.
            Assert.True(workItem.CanWriteDone);
            Assert.False(workItem.CanWriteTitle);

            workItem.Done = true;
            var pushResult = await session.PushAsync();

            Assert.False(pushResult.HasErrors);

            using var transaction = this.Profile.Database.CreateTransaction();
            Assert.True(((DatabaseWorkItem)transaction.Instantiate(workItem.Id)).Done);
        }

        [Fact]
        public async Task AnAdministratorAssignsANewWorkItem()
        {
            var workspace = await this.Profile.CreateWorkspace("admin@example.com");
            var m = workspace.Services.Get<M>();
            var session = workspace.CreateSession();

            var result = await session.PullAsync(new Pull
            {
                Extent = new Filter(m.Person) { Predicate = new Equals(m.Person.UserName) { Value = "jane@example.com" } },
            });

            var jane = Assert.Single(result.GetCollection<Person>());
            Assert.Equal(1, jane.OpenWorkItemCount);

            var workItem = session.Create<WorkItem>();
            workItem.Title = "Review the rule";
            workItem.Assignee = jane;

            var pushResult = await session.PushAsync();

            Assert.False(pushResult.HasErrors);

            // The push stored the work item, and the database derived the number of open work items of its
            // assignee.
            await session.PullAsync(new Pull { Object = jane });
            Assert.Equal(2, jane.OpenWorkItemCount);

            using var transaction = this.Profile.Database.CreateTransaction();
            Assert.Equal("Review the rule", ((DatabaseWorkItem)transaction.Instantiate(workItem.Id)).Title);
        }

        [Fact]
        public async Task APushReportsTheErrorsOfTheDerivation()
        {
            var workspace = await this.Profile.CreateWorkspace("admin@example.com");
            var m = workspace.Services.Get<M>();
            var session = workspace.CreateSession();

            // A work item requires a title.
            session.Create<WorkItem>();
            var pushResult = await session.PushAsync();

            Assert.True(pushResult.HasErrors);
            var error = Assert.Single(pushResult.DerivationErrors);
            Assert.Contains(error.Roles, v => v.RelationType == m.WorkItem.Title.RelationType);

            // The database stored nothing.
            using var transaction = this.Profile.Database.CreateTransaction();
            Assert.Single(new DatabaseWorkItems(transaction).Extent());
        }
    }
}
