// <copyright file="TreeNodeExtensions.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Protocol.Data
{
    using Meta;

    public static class TreeNodeExtensions
    {
        public static void Load(this Node @this, ISession session, Allors.Data.Node node)
        {
            if (@this.Nodes != null)
            {
                var metaPopulation = session.Database.ObjectFactory.MetaPopulation;

                foreach (var child in @this.Nodes)
                {
                    var childPropertyType = (IPropertyType)metaPopulation.FindAssociationType(child.AssociationType) ?? metaPopulation.FindRoleType(child.RoleType);
                    var childTreeNode = new Allors.Data.Node(childPropertyType);
                    node.Add(childTreeNode);
                    child.Load(session, childTreeNode);
                }
            }
        }
    }
}
