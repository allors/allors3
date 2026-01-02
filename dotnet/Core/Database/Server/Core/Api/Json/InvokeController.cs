// <copyright file="DatabaseController.cs" company="Allors bv">
// Copyright (c) Allors bv. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Database.Protocol.Json
{
    using System.Threading;
    using Allors.Protocol.Json.Api.Invoke;
    using Allors.Services;
    using Microsoft.AspNetCore.Authorization;
    using Microsoft.AspNetCore.Mvc;

    [ApiController]
    [Route("allors/invoke")]
    public class InvokeController : ControllerBase
    {
        public InvokeController(ITransactionService transactionService, IWorkspaceService workspaceService, IPolicyService policyService)
        {
            this.TransactionService = transactionService;
            this.WorkspaceService = workspaceService;
            this.PolicyService = policyService;
        }

        private ITransactionService TransactionService { get; }

        public IWorkspaceService WorkspaceService { get; }

        private IPolicyService PolicyService { get; }

        [HttpPost]
        [Authorize]
        [AllowAnonymous]
        public ActionResult<InvokeResponse> Post(InvokeRequest invokeRequest, CancellationToken cancellationToken) =>
            this.PolicyService.InvokePolicy.Execute(
                () =>
                    {
                        using var transaction = this.TransactionService.Transaction;
                        var api = new Api(transaction, this.WorkspaceService.Name, cancellationToken);
                        return api.Invoke(invokeRequest);
                    });
    }
}
