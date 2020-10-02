// <copyright file="DatabaseController.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Server
{
    using System;

    using Allors.Domain;
    using Allors.Protocol.Remote.Invoke;
    using Allors.Services;
    using Api.Json;
    using Api.Json.Invoke;
    using Microsoft.AspNetCore.Authorization;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.Extensions.Logging;

    [ApiController]
    [Route("allors/invoke")]
    public class InvokeController : ControllerBase
    {
        public InvokeController(IDatabaseService databaseService, IWorkspaceService workspaceService, IPolicyService policyService, ILogger<InvokeController> logger)
        {
            this.DatabaseService = databaseService;
            this.WorkspaceService = workspaceService;
            this.PolicyService = policyService;
            this.Logger = logger;
        }

        private IDatabaseService DatabaseService { get; }

        public IWorkspaceService WorkspaceService { get; }

        private IPolicyService PolicyService { get; }

        private ILogger<InvokeController> Logger { get; }

        [HttpPost]
        [Authorize]
        [AllowAnonymous]
        public ActionResult<InvokeResponse> Post(InvokeRequest invokeRequest) =>
            this.PolicyService.InvokePolicy.Execute(
                () =>
                    {
                        try
                        {
                            using var session = this.DatabaseService.Database.CreateSession();
                            var api = new Api(session, this.WorkspaceService.Name);
                            return api.Invoke(invokeRequest);
                        }
                        catch (Exception e)
                        {
                            this.Logger.LogError(e, "InvokeRequest {request}", invokeRequest);
                            throw;
                        }
                    });
    }
}
