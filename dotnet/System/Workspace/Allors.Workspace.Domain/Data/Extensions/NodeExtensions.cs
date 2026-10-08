// <copyright file="NodeExtensions.cs" company="Allors bv">
// Copyright (c) Allors bv. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Workspace.Data
{
    using System.Collections;
    using System.Collections.Generic;

    public static class NodeExtensions
    {
        /// <summary>
        /// The objects at the end of the node's path from the given object, through the roles
        /// and associations of the session.
        /// </summary>
        public static IEnumerable<IObject> Resolve(this Node @this, IObject @object)
        {
            if (@this.PropertyType.IsOne)
            {
                var resolved = @this.PropertyType.Get(@object.Strategy, @this.OfType);
                if (resolved != null)
                {
                    if (@this.Nodes.Length > 0)
                    {
                        foreach (var node in @this.Nodes)
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
                var resolved = (IEnumerable)@this.PropertyType.Get(@object.Strategy, @this.OfType);
                if (resolved != null)
                {
                    if (@this.Nodes.Length > 0)
                    {
                        foreach (var resolvedItem in resolved)
                        {
                            foreach (var node in @this.Nodes)
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
    }
}
