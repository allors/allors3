// <copyright file="ObjectTests.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Tests
{
    using Allors.Workspace.Data;
    using Allors.Workspace.Meta;
    using Xunit;

    public class NodeBuilderTests : Test
    {
        [Fact]
        public void Class()
        {
            var builder = new OrganisationNodeBuilder(M, organisation =>
            {
                organisation.Manager(manage =>
                {
                    manage.Photo();
                    manage.Address(address => address.HomeAddress_AddressablesWhereAddress());
                });
            });

            Node[] nodes = builder;

            Assert.Single(nodes);
            var managerNode = nodes[0];

            Assert.Equal(M.Organisation.Manager, managerNode.PropertyType);

            Assert.Equal(2, managerNode.Nodes.Length);

            var photoNode = managerNode.Nodes[0];
            var addressNode = managerNode.Nodes[1];

            Assert.Equal(M.Person.Photo, photoNode.PropertyType);
            Assert.Equal(M.Person.Address, addressNode.PropertyType);
        }

        [Fact]
        public void Interface()
        {
            var builder = new DeletableNodeBuilder(M, deletable =>
            {
                deletable.Media_MediaContent();
            });

            Node[] nodes = builder;

            Assert.Single(nodes);

            var mediaContentNode = nodes[0];

            Assert.Equal(M.Media.MediaContent, mediaContentNode.PropertyType);
        }
    }
}
