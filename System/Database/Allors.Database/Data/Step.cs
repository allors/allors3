// <copyright file="Step.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Database.Data
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Text;
    using Allors.Database.Meta;
    using Security;

    public class Step : IVisitable
    {
        public Step()
        {
        }

        public Step(params IPropertyType[] propertyTypes) : this(propertyTypes, 0)
        {
        }

        internal Step(IPropertyType[] propertyTypes, int index)
        {
            this.PropertyType = propertyTypes[index];

            var nextIndex = index + 1;
            if (nextIndex < propertyTypes.Length)
            {
                this.Next = new Step(propertyTypes, nextIndex);
            }
        }

        public bool IsOne
        {
            get
            {
                if (this.PropertyType.IsMany)
                {
                    return false;
                }

                return this.ExistNext ? this.Next.IsOne : this.PropertyType.IsOne;
            }
        }

        public Node[] Include { get; set; }

        public IPropertyType PropertyType { get; set; }

        public Step Next { get; set; }

        public bool ExistNext => this.Next != null;

        public Step End => this.ExistNext ? this.Next.End : this;

        public IEnumerable<IObject> Get(IObject @object)
        {
            if (this.PropertyType.IsOne)
            {
                var resolved = this.PropertyType.Get(@object.Strategy);
                if (resolved != null)
                {
                    if (this.ExistNext)
                    {
                        foreach (var next in this.Next.Get((IObject)resolved))
                        {
                            yield return next;
                        }
                    }
                    else
                    {
                        yield return (IObject)this.PropertyType.Get(@object.Strategy);
                    }
                }
            }
            else
            {
                var resolved = (IEnumerable)this.PropertyType.Get(@object.Strategy);
                if (resolved != null)
                {
                    if (this.ExistNext)
                    {
                        foreach (var resolvedItem in resolved)
                        {
                            foreach (var next in this.Next.Get((IObject)resolvedItem))
                            {
                                yield return next;
                            }
                        }
                    }
                    else
                    {
                        foreach (IObject child in (Database.Extent)this.PropertyType.Get(@object.Strategy))
                        {
                            yield return child;
                        }
                    }
                }
            }
        }

        public object Get(IObject @object, IAccessControlLists acls)
        {
            var acl = acls[@object];
            // TODO: Access check for AssociationType
            if (this.PropertyType is IAssociationType || acl.CanRead((IRoleType)this.PropertyType))
            {
                if (this.ExistNext)
                {
                    var currentValue = this.PropertyType.Get(@object.Strategy);

                    if (currentValue != null)
                    {
                        if (currentValue is IObject value)
                        {
                            return this.Next.Get(value, acls);
                        }

                        var results = new HashSet<object>();
                        foreach (var item in (IEnumerable)currentValue)
                        {
                            var nextValueResult = this.Next.Get((IObject)item, acls);
                            if (nextValueResult is HashSet<object> set)
                            {
                                results.UnionWith(set);
                            }
                            else
                            {
                                results.Add(nextValueResult);
                            }
                        }

                        return results;
                    }
                }

                return this.PropertyType.Get(@object.Strategy);
            }

            return null;
        }

        public bool Set(IObject @object, IAccessControlLists acls, object value)
        {
            var acl = acls[@object];
            if (this.ExistNext)
            {
                // TODO: Access check for AssociationType
                if (this.PropertyType is IAssociationType || acl.CanRead((IRoleType)this.PropertyType))
                {
                    if (this.PropertyType.Get(@object.Strategy) is IObject property)
                    {
                        this.Next.Set(property, acls, value);
                        return true;
                    }
                }

                return false;
            }

            if (this.PropertyType is IRoleType roleType)
            {
                if (acl.CanWrite(roleType))
                {
                    roleType.Set(@object.Strategy, value);
                    return true;
                }
            }

            return false;
        }

        public void Ensure(IObject @object, IAccessControlLists acls)
        {
            var acl = acls[@object];

            if (this.PropertyType is IRoleType roleType)
            {
                if (roleType.IsMany)
                {
                    throw new NotSupportedException("RoleType with muliplicity many");
                }

                if (roleType.ObjectType.IsComposite)
                {
                    if (acl.CanRead(roleType))
                    {
                        var role = roleType.Get(@object.Strategy);
                        if (role == null)
                        {
                            if (acl.CanWrite(roleType))
                            {
                                role = @object.Strategy.Session.Create((IClass)roleType.ObjectType);
                                roleType.Set(@object.Strategy, role);
                            }
                        }

                        if (this.ExistNext)
                        {
                            if (role is IObject next)
                            {
                                this.Next.Ensure(next, acls);
                            }
                        }
                    }
                }
            }
            else
            {
                var associationType = (IAssociationType)this.PropertyType;
                if (associationType.IsMany)
                {
                    throw new NotSupportedException("AssociationType with multiplicity many");
                }

                // TODO: Access check for AssociationType
                if (associationType.Get(@object.Strategy) is IObject association)
                {
                    if (this.ExistNext)
                    {
                        this.Next.Ensure(association, acls);
                    }
                }
            }
        }


        public IObjectType GetObjectType()
        {
            if (this.ExistNext)
            {
                return this.Next.GetObjectType();
            }

            return this.PropertyType.ObjectType;
        }
        
        public override string ToString()
        {
            var name = new StringBuilder();
            name.Append(this.PropertyType.Name);
            if (this.ExistNext)
            {
                this.Next.ToStringAppendToName(name);
            }

            return name.ToString();
        }

        private void ToStringAppendToName(StringBuilder name)
        {
            name.Append("." + this.PropertyType.Name);

            if (this.ExistNext)
            {
                this.Next.ToStringAppendToName(name);
            }
        }

        public void Accept(IVisitor visitor) => visitor.VisitStep(this);
    }
}
