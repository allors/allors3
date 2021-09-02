// <copyright file="Select.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Database.Data
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using Meta;
    using Security;

    public class Select : IVisitable
    {
        public Select()
        {
        }

        public Select(params IPropertyType[] propertyTypes) : this(propertyTypes, 0)
        {
        }

        internal Select(IPropertyType[] propertyTypes, int index)
        {
            if (propertyTypes?.Length > 0)
            {
                this.PropertyType = propertyTypes[index];

                var nextIndex = index + 1;
                if (nextIndex < propertyTypes.Length)
                {
                    this.Next = new Select(propertyTypes, nextIndex);
                }
            }
        }

        public Node[] Include { get; set; }

        public IPropertyType PropertyType { get; set; }

        public Select Next { get; set; }

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

        public bool ExistNext => this.Next != null;

        public Select End => this.ExistNext ? this.Next.End : this;

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
                        yield return (IObject)resolved;
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
                        foreach (IObject child in resolved)
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

            if (this.PropertyType is IRoleType roleType && acl.CanWrite(roleType))
            {
                roleType.Set(@object.Strategy, value);
                return true;
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
                    throw new NotSupportedException("RoleType with multiplicity many");
                }

                if (roleType.ObjectType.IsComposite && acl.CanRead(roleType))
                {
                    var role = roleType.Get(@object.Strategy);
                    if (role == null && acl.CanWrite(roleType))
                    {
                        role = @object.Strategy.Transaction.Create((IClass)roleType.ObjectType);
                        roleType.Set(@object.Strategy, role);
                    }

                    if (this.ExistNext && role is IObject next)
                    {
                        this.Next.Ensure(next, acls);
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
                if (associationType.Get(@object.Strategy) is IObject association && this.ExistNext)
                {
                    this.Next.Ensure(association, acls);
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

        public void Accept(IVisitor visitor) => visitor.VisitSelect(this);

        public static bool TryParse(IComposite composite, string selectString, out Select @select)
        {
            var propertyType = Resolve(composite, selectString);
            @select = propertyType == null ? null : new Select(propertyType);
            return @select != null;
        }

        private static IPropertyType Resolve(IComposite composite, string propertyName)
        {
            var lowerCasePropertyName = propertyName.ToLowerInvariant();

            foreach (var roleType in composite.DatabaseRoleTypes)
            {
                if (roleType.SingularName.ToLowerInvariant().Equals(lowerCasePropertyName) ||
                    roleType.SingularFullName.ToLowerInvariant().Equals(lowerCasePropertyName) ||
                    roleType.PluralName.ToLowerInvariant().Equals(lowerCasePropertyName) ||
                    roleType.PluralFullName.ToLowerInvariant().Equals(lowerCasePropertyName))
                {
                    return roleType;
                }
            }

            foreach (var associationType in composite.DatabaseAssociationTypes)
            {
                if (associationType.SingularName.ToLowerInvariant().Equals(lowerCasePropertyName) ||
                    associationType.SingularFullName.ToLowerInvariant().Equals(lowerCasePropertyName) ||
                    associationType.PluralName.ToLowerInvariant().Equals(lowerCasePropertyName) ||
                    associationType.PluralFullName.ToLowerInvariant().Equals(lowerCasePropertyName))
                {
                    return associationType;
                }
            }

            return null;
        }
    }
}
