// <copyright file="TreeNode.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Database.Data
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Linq;
    using Meta;
    using Security;

    public class Node : IVisitable
    {
        public Node(IPropertyType propertyType, IEnumerable<Node> nodes = null)
        {
            this.PropertyType = propertyType;
            this.Composite = this.PropertyType.ObjectType.IsComposite ? (IComposite)propertyType.ObjectType : null;

            if (propertyType.ObjectType.IsComposite)
            {
                this.Nodes = nodes?.Select(this.AssertAssignable).ToArray();
            }

            this.Nodes ??= new Node[0];
        }

        public IPropertyType PropertyType { get; }

        public IComposite Composite { get; }

        public Node[] Nodes { get; private set; }

        public IComposite OfType { get; set; }

        public IEnumerable<IObject> Resolve(IObject @object)
        {
            if (this.PropertyType.IsOne)
            {
                var resolved = this.PropertyType.Get(@object.Strategy, this.OfType);
                if (resolved != null)
                {
                    if (this.Nodes.Length > 0)
                    {
                        foreach (var node in this.Nodes)
                        {
                            foreach (var next in node.Resolve((IObject)resolved))
                            {
                                yield return next;
                            }
                        }
                    }
                    else
                    {
                        yield return (IObject)resolved;
                    }
                }
            }
            else
            {
                var resolved = (IEnumerable)this.PropertyType.Get(@object.Strategy, this.OfType);
                if (resolved != null)
                {
                    if (this.Nodes.Length > 0)
                    {
                        foreach (var resolvedItem in resolved)
                        {
                            foreach (var node in this.Nodes)
                            {
                                foreach (var next in node.Resolve((IObject)resolvedItem))
                                {
                                    yield return next;
                                }
                            }
                        }
                    }
                    else
                    {
                        foreach (IObject child in resolved)
                        {
                            yield return child;
                        }
                    }
                }
            }
        }

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
                                    _ = objects.Add(role);
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
                                    _ = objects.Add(role);
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
                                _ = objects.Add(association);
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
                                _ = objects.Add(association);
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
                _ = prefetchPolicyBuilder.WithRule(this.PropertyType);
            }
            else
            {
                var nestedPrefetchPolicyBuilder = new PrefetchPolicyBuilder();
                foreach (var node in this.Nodes)
                {
                    node.BuildPrefetchPolicy(nestedPrefetchPolicyBuilder);
                }

                var nestedPrefetchPolicy = nestedPrefetchPolicyBuilder.Build();
                _ = prefetchPolicyBuilder.WithRule(this.PropertyType, nestedPrefetchPolicy);
            }
        }

        public Node Add(IEnumerable<IPropertyType> propertyTypes)
        {
            foreach (var propertyType in propertyTypes)
            {
                _ = this.Add(propertyType);
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

        public void Add(Node node) => this.Nodes = this.Nodes.Append(this.AssertAssignable(node)).ToArray();

        private Node AssertAssignable(Node node)
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

            return node;
        }

        public void Accept(IVisitor visitor) => visitor.VisitNode(this);
    }
}
