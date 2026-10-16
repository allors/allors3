// <copyright file="AffectedObjectKind.cs" company="Allors bv">
// Copyright (c) Allors bv. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Database.Adapters.Sql
{
    using System;

    [Flags]
    public enum AffectedObjectKind
    {
        Submitted = 1,
        Created = 2,
        Deleted = 4,
        Role = 8,
        Association = 16,
    }
}
