// <copyright file="NullableArraySet.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Workspace.Adapters.Remote
{
    using System;

    internal static class NullableArraySet
    {
        internal static WorkspaceObject[] Add(object set, WorkspaceObject item) => Add((WorkspaceObject[])set, item);

        internal static WorkspaceObject[] Add(WorkspaceObject[] set, WorkspaceObject item)
        {
            if (set == null)
            {
                return new[] { item };
            }

            Array.Resize(ref set, set.Length + 1);
            set[set.Length - 1] = item;
            return set;
        }

        internal static WorkspaceObject[] Remove(object set, WorkspaceObject item) => Remove((WorkspaceObject[])set, item);

        internal static WorkspaceObject[] Remove(WorkspaceObject[] set, WorkspaceObject item)
        {
            if (set != null && Array.IndexOf(set, item) > -1)
            {
                if (set.Length == 1)
                {
                    return null;
                }

                var index = Array.IndexOf(set, item);
                set[index] = set[set.Length - 1];
                Array.Resize(ref set, set.Length - 1);
                return set;
            }

            return null;
        }
    }

}
