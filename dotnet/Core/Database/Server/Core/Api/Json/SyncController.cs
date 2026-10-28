// <copyright file="DatabaseController.cs" company="Allors bv">
// Copyright (c) Allors bv. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Database.Protocol.Json
{
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
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
        public ActionResult<SyncResponse> Post([FromBody, KnownAccessVersions]SyncRequest syncRequest, CancellationToken cancellationToken) =>
            this.PolicyService.SyncPolicy.Execute(
                () =>
                {
                    using var transaction = this.TransactionService.Transaction;
                    var api = new Api(transaction, this.WorkspaceService.Name, cancellationToken, this.ApiLogger);
                    return api.Sync(syncRequest);
                });

        private sealed class KnownAccessVersionsAttribute : ValidationAttribute
        {
            protected override ValidationResult IsValid(object value, ValidationContext validationContext)
            {
                if (value is not SyncRequest request) return ValidationResult.Success;

                var invalidEntries = new List<string>();
                foreach (var (name, entries) in new[] { (nameof(SyncRequest.g), request.g), (nameof(SyncRequest.r), request.r) })
                {
                    if (entries == null) continue;

                    for (var i = 0; i < entries.Length; ++i)
                    {
                        var entry = entries[i];
                        if (entry == null || entry.Length != 2 || entry[0] <= 0 || entry[1] < 0)
                        {
                            invalidEntries.Add($"{name}[{i}]");
                        }
                    }
                }

                return invalidEntries.Count == 0
                    ? ValidationResult.Success
                    : new ValidationResult("Sync cache entries must be [id, version] pairs with a positive ID and a non-negative version.", invalidEntries);
            }
        }
    }
}
