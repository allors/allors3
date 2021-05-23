// <copyright file="StepExtensions.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Workspace.Domain
{
    using System.Collections;
    using System.Collections.Generic;

    using Allors.Workspace.Data;

    public static class StepExtensions
    {
        public static IEnumerable<IObject> Get(this Step step, IObject @object)
        {
            if (step.PropertyType.IsOne)
            {
                var resolved = step.PropertyType.Get(@object.Strategy);
                if (resolved != null)
                {
                    if (step.ExistNext)
                    {
                        foreach (var next in step.Next.Get((IObject)resolved))
                        {
                            yield return next;
                        }
                    }
                    else
                    {
                        yield return (IObject)step.PropertyType.Get(@object.Strategy);
                    }
                }
            }
            else
            {
                var resolved = (IEnumerable)step.PropertyType.Get(@object.Strategy);
                if (resolved != null)
                {
                    if (step.ExistNext)
                    {
                        foreach (var resolvedItem in resolved)
                        {
                            foreach (var next in step.Next.Get((IObject)resolvedItem))
                            {
                                yield return next;
                            }
                        }
                    }
                    else
                    {
                        foreach (var child in (IEnumerable<IObject>)step.PropertyType.Get(@object.Strategy))
                        {
                            yield return child;
                        }
                    }
                }
            }
        }
    }
}
