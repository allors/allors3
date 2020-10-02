// <copyright file="PullController.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Server.Controllers
{
    using System;
    using Allors.Domain;
    using Allors.Protocol.Data;
    using Allors.Protocol.Remote.Pull;
    using Allors.Services;
    using Api.Json;
    using Microsoft.AspNetCore.Authorization;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.Extensions.Logging;

    [ApiController]
    [Route("allors/pull")]
    public class PullController : ControllerBase
    {
        public PullController(IDatabaseService databaseService, IWorkspaceService workspaceService, IPolicyService policyService, ILogger<PullController> logger)
        {
            this.DatabaseService = databaseService;
            this.WorkspaceService = workspaceService;
            this.PolicyService = policyService;

            var scope = this.DatabaseService.Database.Scope();

            this.ExtentService = scope.ExtentService;
            this.FetchService = scope.FetchService;
            this.TreeService = scope.TreeService;
            this.Logger = logger;
        }

        private IDatabaseService DatabaseService { get; }

        public IWorkspaceService WorkspaceService { get; }

        private IExtentService ExtentService { get; }

        private IFetchService FetchService { get; }

        private ILogger<PullController> Logger { get; }

        private IPolicyService PolicyService { get; }

        private ITreeService TreeService { get; }

        [HttpPost]
        [Authorize]
        [AllowAnonymous]
        public ActionResult<PullResponse> Post([FromBody] PullRequest request) =>
            this.PolicyService.InvokePolicy.Execute(
                () =>
                {
                    try
                    {
                        using var session = this.DatabaseService.Database.CreateSession();
                        var api = new Api(session, this.WorkspaceService.Name);
                        return api.Pull(request);
                    }
                    catch (Exception e)
                    {
                        this.Logger.LogError(e, "PullRequest {request}", request);
                        throw;
                    }
                });
    }
}
