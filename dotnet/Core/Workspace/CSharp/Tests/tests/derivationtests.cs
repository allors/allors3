// <copyright file="DerivationNodesTest.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>
// <summary>
//
// </summary>

namespace Tests.Workspace
{
    using System.Linq;
    using Allors.Workspace;
    using Allors.Workspace.Data;
    using Allors.Workspace.Derivations;
    using Allors.Workspace.Domain;
    using Xunit;

    public abstract class DerivationTests : Test
    {
        protected DerivationTests(Fixture fixture) : base(fixture) { }

        [Fact]
        public async void SessionFullName()
        {
            await this.Login("administrator");

            var pull = new[]
            {
                new Pull
                {
                    Extent = new Filter(this.M.Person)
                }
            };

            var session = this.Workspace.CreateSession();
            session.Activate(this.Workspace.Configuration.Rules);
            var result = await session.PullAsync(pull);

            var people = result.GetCollection<Person>();

            var person = people.First(v => "Jane".Equals(v.FirstName));

            Assert.Equal($"Jane Doe", person.SessionFullName);
        }
    }
}
