// <copyright file="IProfile.cs" company="Allors bv">
// Copyright (c) Allors bv. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Workspace.Tests
{
    using System;
    using System.Threading.Tasks;
    using Allors.Database;
    using Allors.Workspace;

    // A database with the population of the tests, and the workspaces of its users through one of the adapters.
    public interface IProfile : IDisposable
    {
        IDatabase Database { get; }

        // A workspace that acts for the user with this user name.
        Task<IWorkspace> CreateWorkspace(string userName);
    }
}
