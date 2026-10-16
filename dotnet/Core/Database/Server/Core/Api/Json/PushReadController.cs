// <copyright file="PushReadController.cs" company="Allors bv">
// Copyright (c) Allors bv. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Database.Protocol.Json
{
    using System;
    using System.IO;
    using System.Text;
    using System.Threading;
    using System.Threading.Tasks;
    using Allors.Services;
    using Microsoft.AspNetCore.Authorization;
    using Microsoft.AspNetCore.Http;
    using Microsoft.AspNetCore.Mvc;

    [Authorize]
    [ApiController]
    [Route("allors/push-read")]
    public sealed class PushReadController : ControllerBase
    {
        private readonly IWorkspaceService workspaceService;
        private readonly IUserResolver userResolver;
        private readonly PushReadExecutor executor;

        public PushReadController(IWorkspaceService workspaceService, IUserResolver userResolver, PushReadExecutor executor = null)
        {
            this.workspaceService = workspaceService;
            this.userResolver = userResolver;
            this.executor = executor;
        }

        [HttpPost]
        public async Task<IActionResult> Post(CancellationToken cancellationToken)
        {
            if (this.executor == null)
            {
                return this.StatusCode(StatusCodes.Status503ServiceUnavailable, "Configure AddAllorsRead with deployment-owned workspace revisions before serving PushRead.");
            }

            cancellationToken.ThrowIfCancellationRequested();
            // Authentication middleware has already completed admission. Capture trusted inputs once,
            // before asynchronous body reads; scope fields never select the workspace or user.
            var workspaceName = this.workspaceService.Name;
            var identity = ReadUserIdentity.Capture(this.User, this.userResolver);

            // Count decoded body bytes: HTTP server limits can also count chunk framing.
            using var body = new MemoryStream();
            var buffer = new byte[8192];
            int count;
            while ((count = await this.Request.Body.ReadAsync(buffer, cancellationToken)) != 0)
            {
                if (body.Length + count > PushReadExecutor.MaxRequestBytes)
                {
                    return this.StatusCode(StatusCodes.Status413PayloadTooLarge, "Reduce the PushRead request to at most 1 MiB.");
                }

                body.Write(buffer, 0, count);
            }

            try
            {
                var request = new UTF8Encoding(false, true).GetString(body.ToArray());
                var response = this.executor.Execute(request, workspaceName, identity.Resolve, cancellationToken);
                // Once Execute observes a commit, cancellation must not replace its receipt.
                return this.Content(response, "application/json", Encoding.UTF8);
            }
            catch (Exception exception) when (exception is FormatException || exception is DecoderFallbackException)
            {
                return this.BadRequest(exception.Message);
            }
        }
    }
}
