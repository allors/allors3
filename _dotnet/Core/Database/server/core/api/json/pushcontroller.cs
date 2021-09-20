// <copyright file="DatabaseController.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Database.Protocol.Json
{
    using System;
    using Allors.Protocol.Json.Api.Push;
    using Allors.Services;
    using Microsoft.AspNetCore.Authorization;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.Extensions.Logging;

    [ApiController]
    [Route("allors/push")]
    public class PushController : ControllerBase
    {
        public PushController(IDatabaseService databaseService, IWorkspaceService workspaceService, IPolicyService policyService, ILogger<PushController> logger)
        {
            this.DatabaseService = databaseService;
            this.WorkspaceService = workspaceService;
            this.PolicyService = policyService;
            this.Logger = logger;
        }

        private IDatabaseService DatabaseService { get; }

        public IWorkspaceService WorkspaceService { get; }

        private IPolicyService PolicyService { get; }

        private ILogger<PushController> Logger { get; }

        [HttpPost]
        [Authorize]
        [AllowAnonymous]
        public ActionResult<PushResponse> Post([FromBody]PushRequest pushRequest) =>
            this.PolicyService.PushPolicy.Execute(
                () =>
                {
                    try
                    {
                        using var transaction = this.DatabaseService.Database.CreateTransaction();
                        var api = new Api(transaction, this.WorkspaceService.Name);
                        return api.Push(pushRequest);
                    }
                    catch (Exception e)
                    {
                        this.Logger.LogError(e, "PushRequest {request}", pushRequest);
                        throw;
                    }
                });
    }
}
