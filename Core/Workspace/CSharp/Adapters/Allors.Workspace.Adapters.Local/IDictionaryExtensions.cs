// <copyright file="NullableArraySet.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Workspace.Adapters.Local
{
    using System.Collections.Generic;
    using Meta;

    internal static class IDictionaryExtensions
    {
        internal static bool TryGet<T>(this IDictionary<IRelationType, object> dictionary, IRelationType key, out T value)
        {
            var result = dictionary.TryGetValue(key, out var v);
            value = (T)v;
            return result;
        }
    }
}
