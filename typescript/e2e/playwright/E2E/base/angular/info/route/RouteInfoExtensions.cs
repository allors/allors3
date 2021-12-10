// <copyright file="Model.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.E2E.Angular.Info
{
    using System.Collections.Generic;

    public static class RouteInfoExtensions
    {
        public static IEnumerable<RouteInfo> Flatten(this IEnumerable<RouteInfo> items)
        {
            var stack = new Stack<RouteInfo>();
            foreach (var item in items)
            {
                stack.Push(item);
            }

            while (stack.Count > 0)
            {
                var current = stack.Pop();
                yield return current;

                var children = current.Children;
                if (children == null)
                {
                    continue;
                }

                foreach (var child in children)
                {
                    stack.Push(child);
                }
            }
        }
    }
}
