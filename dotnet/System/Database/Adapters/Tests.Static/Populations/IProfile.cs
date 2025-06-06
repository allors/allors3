// <copyright file="IProfile.cs" company="Allors bv">
// Copyright (c) Allors bv. All rights reserved.
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
