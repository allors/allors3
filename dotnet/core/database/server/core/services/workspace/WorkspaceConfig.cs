// <copyright file="DatabaseService.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Services
{
    using System.Collections.Generic;
    using Microsoft.AspNetCore.Http;

    public class WorkspaceConfig
    {
        private readonly IReadOnlyDictionary<HostString, string> workspaceNameByHostString;

        public WorkspaceConfig(IReadOnlyDictionary<HostString, string> workspaceNameByHostString) => this.workspaceNameByHostString = workspaceNameByHostString;

        public string Map(in HostString hostString) => this.workspaceNameByHostString[hostString];
    }
}
