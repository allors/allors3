// <copyright file="NullableArraySet.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Workspace.Adapters.Remote
{
    using System;

    internal static class NullableSortableArraySet
    {
        internal static RemoteStrategy[] Add(object set, RemoteStrategy item) => Add((RemoteStrategy[])set, item);

        internal static RemoteStrategy[] Add(RemoteStrategy[] sourceArray, RemoteStrategy item)
        {
            if (item == null)
            {
                return sourceArray;
            }

            if (sourceArray == null)
            {
                return new[] { item };
            }

            if (Array.IndexOf(sourceArray, item) >= 0)
            {
                return sourceArray;
            }

            var destinationArray = new RemoteStrategy[sourceArray.Length + 1];

            Array.Copy(sourceArray, destinationArray, sourceArray.Length);
            destinationArray[destinationArray.Length - 1] = item;

            return destinationArray;
        }

        internal static RemoteStrategy[] Remove(object set, RemoteStrategy item) => Remove((RemoteStrategy[])set, item);

        internal static RemoteStrategy[] Remove(RemoteStrategy[] sourceArray, RemoteStrategy item)
        {
            if (sourceArray == null || item == null)
            {
                return null;
            }

            var index = Array.IndexOf(sourceArray, item);


            if (index < 0)
            {
                return sourceArray;
            }

            if (sourceArray.Length == 1)
            {
                return null;
            }

            var destinationArray = new RemoteStrategy[sourceArray.Length - 1];

            if (index > 0)
            {
                Array.Copy(sourceArray, 0, destinationArray, 0, index);
            }

            if (index < sourceArray.Length - 1)
            {
                Array.Copy(sourceArray, index + 1, destinationArray, index, sourceArray.Length - index - 1);
            }

            return destinationArray;
        }
    }

}
