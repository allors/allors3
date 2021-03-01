// <copyright file="NullableArraySet.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Workspace.Adapters
{
    using System;

    internal static class NullableArrayList
    {
        internal static Identity[] Add(object set, Identity item) => Add((Identity[])set, item);

        internal static Identity[] Add(Identity[] set, Identity item)
        {
            if (set == null)
            {
                return new[] { item };
            }

            Array.Resize(ref set, set.Length + 1);
            set[set.Length - 1] = item;
            return set;
        }

        internal static Identity[] Remove(object set, Identity item) => Remove((Identity[])set, item);

        internal static Identity[] Remove(Identity[] set, Identity item)
        {
            if (set != null)
            {
                var index = Array.IndexOf(set, item);
                if (index > -1)
                {
                    if (set.Length == 1)
                    {
                        return null;
                    }

                    set[index] = set[set.Length - 1];
                    Array.Resize(ref set, set.Length - 1);
                    return set;
                }
            }

            return null;
        }
    }

}
