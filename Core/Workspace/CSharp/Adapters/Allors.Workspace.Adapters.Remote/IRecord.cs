// <copyright file="IRecord.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Workspace.Adapters.Remote
{
    using Meta;

    internal interface IRecord
    {
        object GetRole(IRoleType roleType);

        long Version { get; }
    }
}
