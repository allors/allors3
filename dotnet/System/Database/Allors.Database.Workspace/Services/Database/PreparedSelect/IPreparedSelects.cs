// <copyright file="IPreparedSelects.cs" company="Allors bv">
// Copyright (c) Allors bv. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Database.Data
{
    using System;

    public partial interface IPreparedSelects
    {
        Select Get(Guid id);
    }
}
