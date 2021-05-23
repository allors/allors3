// <copyright file="DatabaseService.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Services
{
    using Microsoft.AspNetCore.Http;

    public class WorkspaceService : IWorkspaceService
    {
        public WorkspaceConfig WorkspaceConfig { get; }

        public WorkspaceService(WorkspaceConfig workspaceConfig, IHttpContextAccessor contextAccessor)
        {
            this.WorkspaceConfig = workspaceConfig;
            this.Request = contextAccessor.HttpContext.Request;
        }

        public HttpRequest Request { get; set; }

        public string Name => this.WorkspaceConfig.Map(this.Request.Host);
    }
}
