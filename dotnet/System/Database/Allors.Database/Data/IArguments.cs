// <copyright file="IExtentArguments.cs" company="Allors bv">
// Copyright (c) Allors bv. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Database.Data
{
    public interface IArguments
    {
        bool HasArgument(string name);

        object ResolveUnit(string tag, string name);

        object[] ResolveUnits(string tag, string name);

        long? ResolveObject(string name);

        long[] ResolveObjects(string name);

        string ResolveMetaObject(string name);
    }
}
