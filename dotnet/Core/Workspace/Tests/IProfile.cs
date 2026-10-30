// <copyright file="Test.cs" company="Allors bv">
// Copyright (c) Allors bv. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Tests.Workspace
{
    using System.Net.Http;
    using System.Threading.Tasks;
    using Allors.Workspace;
    using Xunit;

    public interface IProfile : IAsyncLifetime
    {
        IWorkspace CreateExclusiveWorkspace();

        IWorkspace CreateWorkspace();

        IWorkspace CreateWorkspace(HttpMessageHandler handler);

        long[] CachedVersions(IWorkspace workspace, long id);

        IWorkspace Workspace { get; }

        Task Login(string userName);

        Task ChangePermissions(long target, string action);

        Task<long> SetupUnitSample(int step);
    }
}
