// <copyright file="FromJson.cs" company="Allors bv">
// Copyright (c) Allors bv. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Database.Protocol.Json
{
    using System.Collections.Generic;

    public interface IResolver
    {
        void Prepare(HashSet<long> objectIds);

        void Resolve(Dictionary<long, IObject> objectById);
    }
}
