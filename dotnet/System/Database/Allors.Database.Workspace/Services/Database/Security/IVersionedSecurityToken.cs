// <copyright file="IBarcodeGenerator.cs" company="Allors bv">
// Copyright (c) Allors bv. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Database.Domain
{
    using System.Collections.Generic;

    public interface IVersionedSecurityToken
    {
        long Id { get; }

        long Version { get; }

        IDictionary<long, long> VersionByGrant { get; }
    }
}
