// <copyright file="TreeExtensions.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Protocol.Data
{
    using System.Collections.Generic;
    using Meta;

    public static class TreeExtensions
    {
        public static Allors.Data.Node[] Load(this Node[] protocolNodes, ISession session)
        {
            var metaPopulation = session.Database.ObjectFactory.MetaPopulation;

            // TODO: Optimize
            var dataNodes = new List<Allors.Data.Node>();

            foreach (var protocolNode in protocolNodes)
            {
                var propertyType = (IPropertyType)metaPopulation.FindAssociationType(protocolNode.AssociationType) ?? metaPopulation.FindRoleType(protocolNode.RoleType);
                var dataNode = new Allors.Data.Node(propertyType);
                dataNodes.Add(dataNode);
                protocolNode.Load(session, dataNode);
            }

            return dataNodes.ToArray();
        }
    }
}
