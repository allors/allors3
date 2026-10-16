// <copyright file="IPendingPull.cs" company="Allors bv">
// Copyright (c) Allors bv. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Workspace
{
    using System.Threading;
    using System.Threading.Tasks;

    public interface IPendingPull
    {
        Task<IPullResult> CompleteAsync(CancellationToken cancellationToken = default);
    }
}
