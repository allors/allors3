// <copyright file="ReadExecutor.cs" company="Allors bv">
// Copyright (c) Allors bv. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Database.Protocol.Json
{
    using System;
    using System.Text;
    using System.Text.Json;
    using System.Threading;
    using Allors.Protocol.Json.Api.Read;
    using Allors.Protocol.Json.SystemTextJson;
    using Security;
    using Microsoft.Extensions.Logging;
    using Microsoft.Extensions.Logging.Abstractions;

    /// <summary>
    /// Executes one captured declarative read. Both transports receive detached, validated JSON only
    /// after the owned SQL attempt has completed, including terminal cleanup. Never retries.
    /// </summary>
    public sealed class ReadExecutor
    {
        public const int MaxRequestBytes = 1024 * 1024;
        public const int MaxResponseBytes = 16 * 1024 * 1024;
        internal const int MaxPoolObjects = 10000;
        internal const int MaxSecurityEntries = 20000;
        internal const int MaxPermissionEntries = 50000;

        private readonly ReadScopeFactory factory;
        private readonly ILogger<ReadExecutor> logger;

        public ReadExecutor(IDatabase database, ReadConfiguration configuration, ILogger<ReadExecutor> logger = null)
        {
            this.factory = new ReadScopeFactory(database, configuration);
            this.logger = logger ?? NullLogger<ReadExecutor>.Instance;
        }

        public string Execute(string rawRequest, string workspaceName, LocalReadIdentity identity, CancellationToken cancellationToken = default)
        {
            ArgumentNullException.ThrowIfNull(identity);
            return this.Execute(rawRequest, request => this.factory.Open(workspaceName, identity, request.x, request.sc, cancellationToken), cancellationToken);
        }

        public string Execute(string rawRequest, string workspaceName, Func<ITransaction, IUser> resolveUser, CancellationToken cancellationToken = default)
        {
            ArgumentNullException.ThrowIfNull(resolveUser);
            return this.Execute(rawRequest, request => this.factory.Open(workspaceName, resolveUser, request.x, request.sc, cancellationToken), cancellationToken);
        }

        private string Execute(string rawRequest, Func<ReadRequest, ReadContext> open, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            if (rawRequest != null && Encoding.UTF8.GetByteCount(rawRequest) > MaxRequestBytes)
            {
                throw new FormatException($"Reduce the read request to at most {MaxRequestBytes} UTF-8 bytes by sending fewer selections or cache hints.");
            }

            // Decode before admission: unknown/duplicate members cannot reach execution.
            var request = ReadJson.DeserializeRequest(rawRequest);
            ReadScope scope = null;
            try
            {
                using var context = open(request);
                scope = context.Scope;
                var unitConvert = new UnitConvert();
                var compiled = ReadCompiler.Compile(context.Transaction, scope.w, request.p, unitConvert);
                context.EnsureValid();
                var result = new ReadResponseBuilder(context, request, compiled, unitConvert).Build();
                var json = JsonSerializer.Serialize(result, result.GetType());
                if (Encoding.UTF8.GetByteCount(json) > MaxResponseBytes)
                {
                    throw new ReadRequestException($"Reduce the read selection or page its results: the response exceeds {MaxResponseBytes} UTF-8 bytes.");
                }

                // This validates even reuse indexes against our private request copy while the scope lives.
                ReadJson.DeserializeResponse(json, request);
                context.Complete();
                cancellationToken.ThrowIfCancellationRequested();
                return json;
            }
            catch (OperationCanceledException)
            {
                throw;
            }
            catch (Exception exception)
            {
                cancellationToken.ThrowIfCancellationRequested();
                this.logger.LogWarning(exception, "Protected workspace read failed before publication.");
                var failure = new ReadFailure
                {
                    i = request.i, k = "error", sc = scope,
                    e = new[] { new ReadError
                    {
                        c = "read-failed",
                        // Domain/provider exceptions can contain private values or connection details.
                        m = exception is ReadRequestException ? exception.Message : "The protected read could not complete. Check the configured population, identity, scope, declarative selection and server diagnostics before starting a new explicit read.",
                    } },
                };
                var json = JsonSerializer.Serialize(failure);
                ReadJson.DeserializeResponse(json, request);
                return json;
            }
        }
    }
}
