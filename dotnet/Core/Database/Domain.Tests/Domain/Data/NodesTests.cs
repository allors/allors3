// <copyright file="FilterTests.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>
// <summary>
//   Defines the ApplicationTests type.
// </summary>

namespace Allors.Database.Data.Tests
{
    using System.Linq;
    using Domain.Tests;
    using Xunit;

    public class NodesTests : DomainTest, IClassFixture<Fixture>
    {
        public NodesTests(Fixture fixture) : base(fixture) { }

        [Fact]
        public void Self()
        {
            var m = this.M;

            var node = m.UserGroup.Members.Node();

            Assert.Equal(m.UserGroup.Members, node.PropertyType);
            Assert.Empty(node.Nodes);
        }

        [Fact]
        public void Child()
        {
            var m = this.M;

            var node = m.UserGroup.Members.Node(v => v.User.UniqueId.Node());

            Assert.Equal(m.UserGroup.Members, node.PropertyType);
            Assert.Single(node.Nodes);

            var child = node.Nodes.First();

            Assert.Equal(m.User.UniqueId, child.PropertyType);
            Assert.Empty(child.Nodes);
        }

        [Fact]
        public void ChildrenArray()
        {
            var m = this.M;

            var node = m.UserGroup.Members.Node(v => new[]
            {
                v.User.UniqueId.Node(),
                v.User.SecurityTokens.Node(),
            });

            Assert.Equal(m.UserGroup.Members, node.PropertyType);
            Assert.Equal(2, node.Nodes.Length);

            var uniqueIdChild = node.Nodes.First(v => v.PropertyType.Equals(m.User.UniqueId));

            Assert.NotNull(uniqueIdChild);
            Assert.Empty(uniqueIdChild.Nodes);

            var securityTokens = node.Nodes.First(v => v.PropertyType.Equals(m.User.SecurityTokens));

            Assert.NotNull(securityTokens);
            Assert.Empty(securityTokens.Nodes);

        }

        [Fact]
        public void ChildrenRest()
        {
            var m = this.M;

            var node = m.UserGroup.Members.Node(
                v => v.User.UniqueId.Node(),
                v => v.User.SecurityTokens.Node());

            Assert.Equal(m.UserGroup.Members, node.PropertyType);
            Assert.Equal(2, node.Nodes.Length);

            var uniqueIdChild = node.Nodes.First(v => v.PropertyType.Equals(m.User.UniqueId));

            Assert.NotNull(uniqueIdChild);
            Assert.Empty(uniqueIdChild.Nodes);

            var securityTokens = node.Nodes.First(v => v.PropertyType.Equals(m.User.SecurityTokens));

            Assert.NotNull(securityTokens);
            Assert.Empty(securityTokens.Nodes);
        }
    }
}
