// <copyright file="TreeNode.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Database.Data
{
    using System;
    using System.Collections.Generic;
    using Meta;
    using Security;

    public static class NodeExtensions
    {
        public static void Resolve(this Node[] treeNodes, ICollection<IObject> collection, IAccessControl acls, Action<IObject> add)
        {
            if (collection == null || collection.Count == 0)
            {
                return;
            }

            foreach (var node in treeNodes)
            {
                foreach (var @object in collection)
                {
                    node.Resolve(@object, acls, add);
                }
            }
        }

        public static void Resolve(this Node[] treeNodes, IObject @object, IAccessControl acls, Action<IObject> add)
        {
            if (@object == null)
            {
                return;
            }

            foreach (var node in treeNodes)
            {
                node.Resolve(@object, acls, add);
            }
        }

        private static void Resolve(this Node @this, IObject @object, IAccessControl acls, Action<IObject> add)
        {
            if (@object == null)
            {
                return;
            }

            var acl = acls[@object];
            // TODO: Access check for AssociationType
            if (!(@this.PropertyType is IAssociationType) && !acl.CanRead((IRoleType)@this.PropertyType))
            {
                return;
            }

            if (@this.PropertyType is IRoleType roleType)
            {
                if (roleType.ObjectType.IsComposite)
                {
                    if (roleType.IsOne)
                    {
                        var role = @object.Strategy.GetCompositeRole(roleType);
                        if (role != null)
                        {
                            add(role);
                            foreach (var node in @this.Nodes)
                            {
                                node.Resolve(role, acls, add);
                            }
                        }
                    }
                    else
                    {
                        foreach (var role in @object.Strategy.GetCompositesRole<IObject>(roleType))
                        {
                            add(role);
                            foreach (var node in @this.Nodes)
                            {
                                node.Resolve(role, acls, add);
                            }
                        }
                    }
                }
            }
            else if (@this.PropertyType is IAssociationType associationType)
            {
                if (associationType.IsOne)
                {
                    var association = @object.Strategy.GetCompositeAssociation(associationType);
                    if (association != null)
                    {
                        add(association);
                        foreach (var node in @this.Nodes)
                        {
                            node.Resolve(association, acls, add);
                        }
                    }
                }
                else
                {
                    foreach (var association in @object.Strategy.GetCompositesAssociation<IObject>(associationType))
                    {
                        add(association);
                        foreach (var node in @this.Nodes)
                        {
                            node.Resolve(association, acls, add);
                        }
                    }
                }
            }
        }
    }
}
