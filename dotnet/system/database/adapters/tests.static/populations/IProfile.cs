// <copyright file="IProfile.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Database.Adapters
{
    using System;

    public interface IProfile : IDisposable
    {
        IDatabase Database { get; }

        ITransaction Transaction { get; }

        Action[] Markers { get; }

        Action[] Inits { get; }

        IDatabase CreateDatabase();
    }
}
