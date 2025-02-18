
// <copyright file="PullController.cs" company="Allors bv">
// Copyright (c) Allors bv. All rights reserved.
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
        public PullController(ITransactionService transactionService, IWorkspaceService workspaceService, IPolicyService policyService)
        {
            this.TransactionService = transactionService;
            this.WorkspaceService = workspaceService;
            this.PolicyService = policyService;
        }

        private ITransactionService TransactionService { get; }

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
                        using var transaction = this.TransactionService.Transaction;
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
