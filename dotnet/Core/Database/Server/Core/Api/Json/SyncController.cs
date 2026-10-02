// <copyright file="DatabaseController.cs" company="Allors bv">
// Copyright (c) Allors bv. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Database.Protocol.Json
{
    using System.Threading;
    using Allors.Protocol.Json.Api.Sync;
    using Allors.Services;
    using Microsoft.AspNetCore.Authorization;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.Extensions.Logging;

    [Authorize]
    [ApiController]
    [Route("allors/sync")]
    public class SyncController : ControllerBase
    {
        public SyncController(ITransactionService transactionService, IWorkspaceService workspaceService, IPolicyService policyService, ILogger<Api> apiLogger = null)
        {
            this.TransactionService = transactionService;
            this.WorkspaceService = workspaceService;
            this.PolicyService = policyService;
            this.ApiLogger = apiLogger;
        }

        private ITransactionService TransactionService { get; }
        public IWorkspaceService WorkspaceService { get; }

        private IPolicyService PolicyService { get; }

        private ILogger<Api> ApiLogger { get; }

        [HttpPost]
        public ActionResult<SyncResponse> Post([FromBody]SyncRequest syncRequest, CancellationToken cancellationToken) =>
            this.PolicyService.SyncPolicy.Execute(
                () =>
                {
                    using var transaction = this.TransactionService.Transaction;
                    var api = new Api(transaction, this.WorkspaceService.Name, cancellationToken, this.ApiLogger);
                    return api.Sync(syncRequest);
                });
    }
}
