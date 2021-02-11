// <copyright file="ObjectTests.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Tests.Workspace
{
    using Allors.Workspace.Data;
    using Allors.Workspace.Meta;
    using Remote;
    using Xunit;

    public abstract class NodeBuilderTests : Test
    {
        protected NodeBuilderTests(Fixture fixture) : base(fixture)
        {
        }


        [Fact]
        public void Class()
        {
            var builder = new OrganisationNodeBuilder(this.M, organisation => organisation.Manager(manager =>
            {
                manager.CycleOne();
                manager.CycleMany(person => person.Employees());
            }));

            Node[] nodes = builder;

            Assert.Single(nodes);
            var managerNode = nodes[0];

            Assert.Equal(this.M.Organisation.Manager, managerNode.PropertyType);

            Assert.Equal(2, managerNode.Nodes.Length);

            var photoNode = managerNode.Nodes[0];
            var addressNode = managerNode.Nodes[1];

            Assert.Equal(this.M.Person.CycleOne, photoNode.PropertyType);
            Assert.Equal(this.M.Person.CycleMany, addressNode.PropertyType);

            // TODO: Check subnodes
        }

        [Fact]
        public void Interface()
        {
            var builder = new C1NodeBuilder(this.M, deletable =>
            {
                deletable.C1C2One2One();
            });

            Node[] nodes = builder;

            Assert.Single(nodes);

            var mediaContentNode = nodes[0];

            Assert.Equal(this.M.C1.C1C2One2One, mediaContentNode.PropertyType);
        }
    }
}
