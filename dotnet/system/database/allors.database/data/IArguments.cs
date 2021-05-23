// <copyright file="IExtentArguments.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Database.Data
{
    using System;

    public interface IArguments
    {
        bool HasArgument(string name);

        object ResolveUnit(int tag, string name);

        object[] ResolveUnits(int tag, string name);

        long ResolveObject(string name);

        long[] ResolveObjects(string name);

        int ResolveMetaObject(string name);
    }
}
