
// <copyright file="PullController.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Database.Protocol.Json
{
    using System;
    using System.Threading;
    using Allors.Protocol.Json.Api.Pull;
    using Allors.Services;
    using Microsoft.AspNetCore.Authorization;
    using Microsoft.AspNetCore.Mvc;
    using NLog;

    [ApiController]
    [Route("allors/pull")]
    public class PullController : ControllerBase
    {
        public PullController(IDatabaseService databaseService, IWorkspaceService workspaceService, IPolicyService policyService)
        {
            this.DatabaseService = databaseService;
            this.WorkspaceService = workspaceService;
            this.PolicyService = policyService;
        }

        private IDatabaseService DatabaseService { get; }

        public IWorkspaceService WorkspaceService { get; }


        public Logger Logger => LogManager.GetCurrentClassLogger();

        private IPolicyService PolicyService { get; }

        [HttpPost]
        [Authorize]
        [AllowAnonymous]
        public ActionResult<PullResponse> Post([FromBody] PullRequest request, CancellationToken cancellationToken) =>
            this.PolicyService.InvokePolicy.Execute(
                () =>
                {
                    try
                    {
                        using var transaction = this.DatabaseService.Database.CreateTransaction();
                        var api = new Api(transaction, this.WorkspaceService.Name, cancellationToken);
                        return api.Pull(request);
                    }
                    catch (Exception e)
                    {
                        this.Logger.Error(e, "PullRequest {request}", request);
                        throw;
                    }
                    finally
                    {
                        cancellationToken.ThrowIfCancellationRequested();
                    }
                });
    }
}
