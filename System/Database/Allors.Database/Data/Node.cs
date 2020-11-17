// <copyright file="TreeNode.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Data
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Allors.Meta;

    public class Node : IVisitable
    {
        public Node(IPropertyType propertyType, Node[] nodes = null)
        {
            this.PropertyType = propertyType;
            this.Composite = this.PropertyType.ObjectType.IsComposite ? (IComposite)propertyType.ObjectType : null;

            if (propertyType.ObjectType.IsComposite)
            {
                if (nodes != null)
                {
                    foreach (var node in nodes)
                    {
                        this.AssertAssignable(node);
                    }
                }

                this.Nodes = nodes ?? new Node[0];
            }
        }

        public IPropertyType PropertyType { get; }

        public IComposite Composite { get; }

        public Node[] Nodes { get; private set; }

        public void Resolve(IObject @object, IAccessControlLists acls, ISet<IObject> objects)
        {
            if (@object != null)
            {
                var acl = acls[@object];
                // TODO: Access check for AssociationType
                if (this.PropertyType is IAssociationType || acl.CanRead((IRoleType)this.PropertyType))
                {
                    if (this.PropertyType is IRoleType roleType)
                    {
                        if (roleType.ObjectType.IsComposite)
                        {
                            if (roleType.IsOne)
                            {
                                var role = @object.Strategy.GetCompositeRole(roleType);
                                if (role != null)
                                {
                                    objects.Add(role);
                                    foreach (var node in this.Nodes)
                                    {
                                        node.Resolve(role, acls, objects);
                                    }
                                }
                            }
                            else
                            {
                                var roles = @object.Strategy.GetCompositeRoles(roleType);
                                foreach (IObject role in roles)
                                {
                                    objects.Add(role);
                                    foreach (var node in this.Nodes)
                                    {
                                        node.Resolve(role, acls, objects);
                                    }
                                }
                            }
                        }
                    }
                    else if (this.PropertyType is IAssociationType associationType)
                    {
                        if (associationType.IsOne)
                        {
                            var association = @object.Strategy.GetCompositeAssociation(associationType);
                            if (association != null)
                            {
                                objects.Add(association);
                                foreach (var node in this.Nodes)
                                {
                                    node.Resolve(association, acls, objects);
                                }
                            }
                        }
                        else
                        {
                            var associations = @object.Strategy.GetCompositeAssociations(associationType);
                            foreach (IObject association in associations)
                            {
                                objects.Add(association);
                                foreach (var node in this.Nodes)
                                {
                                    node.Resolve(association, acls, objects);
                                }
                            }
                        }
                    }
                }
            }
        }

        public void BuildPrefetchPolicy(PrefetchPolicyBuilder prefetchPolicyBuilder)
        {
            if (this.Nodes == null || this.Nodes.Length == 0)
            {
                prefetchPolicyBuilder.WithRule(this.PropertyType);
            }
            else
            {
                var nestedPrefetchPolicyBuilder = new PrefetchPolicyBuilder();
                foreach (var node in this.Nodes)
                {
                    node.BuildPrefetchPolicy(nestedPrefetchPolicyBuilder);
                }

                var nestedPrefetchPolicy = nestedPrefetchPolicyBuilder.Build();
                prefetchPolicyBuilder.WithRule(this.PropertyType, nestedPrefetchPolicy);
            }
        }

        public Node Add(IEnumerable<IPropertyType> propertyTypes)
        {
            foreach (var propertyType in propertyTypes)
            {
                this.Add(propertyType);
            }

            return this;
        }

        public Node Add(IPropertyType propertyType)
        {
            var treeNode = new Node(propertyType);
            this.Add(treeNode);
            return this;
        }

        public Node Add(IPropertyType propertyType, Node[] subTree)
        {
            var treeNode = new Node(propertyType, subTree);
            this.Add(treeNode);
            return this;
        }

        public void Add(Node node)
        {
            this.AssertAssignable(node);
            this.Nodes = this.Nodes.Append(node).ToArray();
        }

        private void AssertAssignable(Node node)
        {
            if (this.Composite != null)
            {
                IComposite addedComposite = null;

                if (node.PropertyType is IRoleType roleType)
                {
                    addedComposite = roleType.AssociationType.ObjectType;
                }
                else if (node.PropertyType is IAssociationType associationType)
                {
                    addedComposite = (IComposite)associationType.RoleType.ObjectType;
                }

                if (addedComposite == null || !(this.Composite.Equals(addedComposite) || this.Composite.DatabaseClasses.Intersect(addedComposite.DatabaseClasses).Any()))
                {
                    throw new ArgumentException(node.PropertyType + " is not a valid tree node on " + this.Composite + ".");
                }
            }
        }

        public void Accept(IVisitor visitor) => visitor.VisitNode(this);
    }
}
