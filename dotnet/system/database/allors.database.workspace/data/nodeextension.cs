// <copyright file="TreeNode.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Database.Data
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Linq;
    using Domain;
    using Meta;
    using Security;

    public static class NodeExtensions
    {
        public static void Resolve(this Node[] treeNodes, ICollection<IObject> collection, IAccessControl acls, Action<IObject> add, IPrefetchPolicyCache prefetchPolicyCache, ITransaction transaction)
        {
            if (treeNodes.Length == 0 || collection == null || collection.Count == 0)
            {
                return;
            }

            var prefetchPolicy = prefetchPolicyCache.ForNodes(treeNodes);
            transaction.Prefetch(prefetchPolicy, collection);

            foreach (var node in treeNodes)
            {
                foreach (var @object in collection)
                {
                    node.Resolve(@object, acls, add, prefetchPolicyCache, transaction);
                }
            }
        }

        public static void Resolve(this Node[] treeNodes, IObject @object, IAccessControl acls, Action<IObject> add, IPrefetchPolicyCache prefetchPolicyCache, ITransaction transaction)
        {
            if (treeNodes.Length == 0 || @object == null)
            {
                return;
            }

            var prefetchPolicy = prefetchPolicyCache.ForNodes(treeNodes);
            transaction.Prefetch(prefetchPolicy, new[] { @object });

            foreach (var node in treeNodes)
            {
                node.Resolve(@object, acls, add, prefetchPolicyCache, transaction);
            }
        }

        private static void Resolve(this Node @this, IObject @object, IAccessControl acls, Action<IObject> add, IPrefetchPolicyCache prefetchPolicyCache, ITransaction transaction)
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
                        var compositeRole = @object.Strategy.GetCompositeRole(roleType);
                        if (compositeRole != null)
                        {
                            add(compositeRole);
                        }

                        @this.Nodes.Resolve(compositeRole, acls, add, prefetchPolicyCache, transaction);
                    }
                    else
                    {
                        var compositesRole = @object.Strategy.GetCompositesRole<IObject>(roleType).ToArray();
                        foreach (var role in compositesRole)
                        {
                            add(role);
                        }

                        @this.Nodes.Resolve(compositesRole, acls, add, prefetchPolicyCache, transaction);
                    }
                }
            }
            else if (@this.PropertyType is IAssociationType associationType)
            {
                if (associationType.IsOne)
                {
                    var compositeAssociation = @object.Strategy.GetCompositeAssociation(associationType);
                    if (compositeAssociation != null)
                    {
                        add(compositeAssociation);
                    }

                    @this.Nodes.Resolve(compositeAssociation, acls, add, prefetchPolicyCache, transaction);
                }
                else
                {
                    var compositesAssociation = @object.Strategy.GetCompositesAssociation<IObject>(associationType)
                        .ToArray();
                    foreach (var association in compositesAssociation)
                    {
                        add(association);
                    }

                    @this.Nodes.Resolve(compositesAssociation, acls, add, prefetchPolicyCache, transaction);
                }
            }
        }
    }
}
