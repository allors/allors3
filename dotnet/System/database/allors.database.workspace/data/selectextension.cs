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

    public static class SelectExtension
    {
        private static bool Match(this Select @this, object value)
        {
            if (@this.OfType != null && value is IObject obj)
            {
                return @this.OfType.IsAssignableFrom(obj.Strategy.Class);
            }

            return true;
        }

        public static object Get(this Select @this, IObject @object, IAccessControl acls)
        {
            var acl = acls[@object];
            // TODO: Access check for AssociationType
            if (@this.PropertyType is IAssociationType || acl.CanRead((IRoleType)@this.PropertyType))
            {
                if (@this.ExistNext)
                {
                    var current = @this.PropertyType.Get(@object.Strategy);

                    switch (current)
                    {
                        case null:
                            return null;
                        case IObject currentObject:
                            return @this.Match(currentObject) ? @this.Next.Get(currentObject, acls) : null;
                        default:
                            var results = new HashSet<object>();
                            foreach (var item in (IEnumerable)current)
                            {
                                if (!@this.Match(item))
                                {
                                    continue;
                                }

                                var nextValueResult = @this.Next.Get((IObject)item, acls);
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

                var selection = @this.PropertyType.Get(@object.Strategy);
                return selection switch
                {
                    null => null,
                    IObject currentObject => @this.Match(currentObject) ? currentObject : null,
                    IEnumerable<IObject> enumerable => enumerable.Where(@this.Match).ToArray(),
                    var unit => unit,
                };
            }

            return null;
        }

        public static bool Set(this Select @this, IObject @object, IAccessControl acls, object value)
        {
            var acl = acls[@object];
            if (@this.ExistNext)
            {
                // TODO: Access check for AssociationType
                if (@this.PropertyType is IAssociationType || acl.CanRead((IRoleType)@this.PropertyType))
                {
                    if (@this.PropertyType.Get(@object.Strategy) is IObject property)
                    {
                        @this.Next.Set(property, acls, value);
                        return true;
                    }
                }

                return false;
            }

            if (@this.PropertyType is IRoleType roleType && acl.CanWrite(roleType))
            {
                roleType.Set(@object.Strategy, value);
                return true;
            }

            return false;
        }

        public static void Ensure(this Select @this, IObject @object, IAccessControl acls)
        {
            var acl = acls[@object];

            if (@this.PropertyType is IRoleType roleType)
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

                    if (@this.ExistNext && role is IObject next)
                    {
                        @this.Next.Ensure(next, acls);
                    }
                }
            }
            else
            {
                var associationType = (IAssociationType)@this.PropertyType;
                if (associationType.IsMany)
                {
                    throw new NotSupportedException("AssociationType with multiplicity many");
                }

                // TODO: Access check for AssociationType
                if (associationType.Get(@object.Strategy) is IObject association && @this.ExistNext)
                {
                    @this.Next.Ensure(association, acls);
                }
            }
        }
    }
}
