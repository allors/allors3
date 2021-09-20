// <copyright file="StepExtensions.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Workspace.Domain
{
    using System.Collections;
    using System.Collections.Generic;

    using Allors.Workspace.Data;

    public static class SelectExtensions
    {
        public static IEnumerable<IObject> Get(this Select @this, IObject @object)
        {
            if (@this.PropertyType.IsOne)
            {
                var resolved = @this.PropertyType.Get(@object.Strategy);
                if (resolved != null)
                {
                    if (@this.ExistNext)
                    {
                        foreach (var next in @this.Next.Get((IObject)resolved))
                        {
                            yield return next;
                        }
                    }
                    else
                    {
                        yield return (IObject)@this.PropertyType.Get(@object.Strategy);
                    }
                }
            }
            else
            {
                var resolved = (IEnumerable)@this.PropertyType.Get(@object.Strategy);
                if (resolved != null)
                {
                    if (@this.ExistNext)
                    {
                        foreach (var resolvedItem in resolved)
                        {
                            foreach (var next in @this.Next.Get((IObject)resolvedItem))
                            {
                                yield return next;
                            }
                        }
                    }
                    else
                    {
                        foreach (var child in (IEnumerable<IObject>)@this.PropertyType.Get(@object.Strategy))
                        {
                            yield return child;
                        }
                    }
                }
            }
        }
    }
}
