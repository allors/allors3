// <copyright file="WorkItemTests.cs" company="Allors bv">
// Copyright (c) Allors bv. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Server.Tests
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using Allors.Database.Domain;
    using Allors.Protocol.Json.Api.Pull;
    using Allors.Protocol.Json.Api.Push;
    using Allors.Protocol.Json.Api.Sync;
    using Allors.Protocol.Json.Data;
    using Xunit;

    // A worker works with the work items through the JSON API, as the security setup of the sample allows: a
    // worker reads work items and marks them done, and reads the names of people.
    public class WorkItemTests : ApiTest
    {
        private const string Password = "correct horse battery";

        private readonly long workItemId;
        private readonly long janeId;

        public WorkItemTests() : base(new Dictionary<string, string>
        {
            ["AllowedHosts"] = "localhost;sample.example",
        })
        {
            using var transaction = this.Database.CreateTransaction();
            var john = new PersonBuilder(transaction).WithUserName("john@example.com").WithInUserPassword(Password).Build();
            new UserGroups(transaction).Workers.AddMember(john);
            var jane = new PersonBuilder(transaction).WithFirstName("Jane").WithLastName("Doe").WithUserName("jane@example.com").Build();
            var workItem = new WorkItemBuilder(transaction).WithTitle("Write the rule").WithAssignee(jane).Build();
            transaction.Derive();
            transaction.Commit();

            this.workItemId = workItem.Id;
            this.janeId = jane.Id;
        }

        [Theory]
        [InlineData("localhost:5000")]
        [InlineData("localhost:6182")]
        [InlineData("sample.example:6182")]
        public async Task AWorkerPullsTheWorkItemsAndSyncsTheRolesThatAWorkerCanRead(string authority)
        {
            this.HttpClient.BaseAddress = new Uri($"http://{authority}/allors/");
            await this.SignIn("john@example.com", Password);

            var pullResponse = await this.Pull(new PullRequest
            {
                l = new[] { new Pull { e = new Extent { k = ExtentKind.Filter, t = this.M.WorkItem.Tag } } },
            });

            Assert.Equal(new[] { this.workItemId }, pullResponse.c["WorkItems"]);

            var syncResponse = await this.Sync(new SyncRequest { o = new[] { this.workItemId, this.janeId } });

            var workItem = syncResponse.o.Single(v => v.i == this.workItemId);
            Assert.Equal("Write the rule", workItem.ro.Single(v => v.t == this.M.WorkItem.Title.RelationType.Tag).v.ToString());
            Assert.Equal(this.janeId, workItem.ro.Single(v => v.t == this.M.WorkItem.Assignee.RelationType.Tag).o);

            var jane = syncResponse.o.Single(v => v.i == this.janeId);
            Assert.Equal("Jane", jane.ro.Single(v => v.t == this.M.Person.FirstName.RelationType.Tag).v.ToString());
            Assert.DoesNotContain(jane.ro, v => v.t == this.M.Person.UserName.RelationType.Tag);
            Assert.DoesNotContain(jane.ro, v => v.t == this.M.Person.OpenWorkItemCount.RelationType.Tag);
        }

        [Fact]
        public async Task AWorkerMarksAWorkItemDone()
        {
            await this.SignIn("john@example.com", Password);
            var version = (await this.Sync(new SyncRequest { o = new[] { this.workItemId } })).o.Single().v;

            var pushResponse = await this.Push(new PushRequest
            {
                o = new[]
                {
                    new PushRequestObject
                    {
                        d = this.workItemId,
                        v = version,
                        r = new[] { new PushRequestRole { t = this.M.WorkItem.Done.RelationType.Tag, u = true } },
                    },
                },
            });

            Assert.False(pushResponse.HasErrors);

            using var transaction = this.Database.CreateTransaction();
            Assert.True(((WorkItem)transaction.Instantiate(this.workItemId)).Done);
        }
    }
}
