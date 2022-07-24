// <copyright file="IDatabaseService.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Services
{
    using System;
    using Database;

    public interface IDatabaseService
    {
        Func<IDatabase> Build { get; set; }

        IDatabase Database { get; set; }
    }
}
