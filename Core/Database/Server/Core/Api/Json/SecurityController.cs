// <copyright file="DatabaseController.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Server
{
    using System;
    using Protocol.Database.Security;
    using Allors.Services;
    using Api.Json;
    using Microsoft.AspNetCore.Authorization;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.Extensions.Logging;

    [ApiController]
    [Route("allors/security")]
    public class SecurityController : ControllerBase
    {
        public SecurityController(IDatabaseService databaseService, IWorkspaceService workspaceService, IPolicyService policyService, ILogger<SecurityController> logger)
        {
            this.DatabaseService = databaseService;
            this.WorkspaceService = workspaceService;
            this.PolicyService = policyService;
            this.Logger = logger;
        }

        private IDatabaseService DatabaseService { get; }
        public IWorkspaceService WorkspaceService { get; }

        private IPolicyService PolicyService { get; }

        private ILogger<SecurityController> Logger { get; }

        [HttpPost]
        [Authorize]
        [AllowAnonymous]
        public ActionResult<SecurityResponse> Post([FromBody]SecurityRequest securityRequest) =>
            this.PolicyService.SyncPolicy.Execute(
                () =>
                {
                    try
                    {
                        using var session = this.DatabaseService.Database.CreateSession();
                        var api = new Api(session, this.WorkspaceService.Name);
                        return api.Security(securityRequest);
                    }
                    catch (Exception e)
                    {
                        this.Logger.LogError(e, "SecurityRequest {request}", securityRequest);
                        throw;
                    }
                });
    }
}
