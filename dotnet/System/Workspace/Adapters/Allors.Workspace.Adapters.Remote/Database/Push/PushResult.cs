// <copyright file="RemotePushResult.cs" company="Allors bv">
// Copyright (c) Allors bv. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Workspace.Adapters.Remote
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Allors.Protocol.Json.Api.Push;

    public class PushResult : Result, IPushResult
    {
        internal PushResult(ISession session, PushResponse response, IEnumerable<CreationMapping> mappings) : base(session, response)
        {
            this.Outcome = response.HasErrors ? MutationOutcome.Rejected : MutationOutcome.Committed;
            var needsExplanation = response.HasErrors && string.IsNullOrWhiteSpace(response._e) &&
                !(response._d?.Any(value => !string.IsNullOrWhiteSpace(value?.m)) ?? false);
            this.Diagnostics = ScopedPushResult.CaptureDiagnostics(response, needsExplanation
                ? new[] { new MutationDiagnostic("push-rejected", "The save was rejected. Inspect its object and derivation diagnostics, correct the changes, and save explicitly.") }
                : null);
            this.CreationMappings = Array.AsReadOnly(mappings.ToArray());
            this.Application = new MutationApplication(response.HasErrors ? MutationAcknowledgementStatus.NotApplicable : MutationAcknowledgementStatus.Applied,
                MutationRefreshApplicationStatus.NotApplicable);
        }

        public MutationOutcome Outcome { get; }
        public MutationDiagnostics Diagnostics { get; }
        public IReadOnlyList<CreationMapping> CreationMappings { get; }
        public MutationRefresh Refresh { get; } = new MutationRefresh(MutationReadStatus.NotRequested);
        public MutationApplication Application { get; }
        public PushDiscovery Discovery { get; } = new PushDiscovery(MutationReadStatus.NotRequested);
        public IPendingPull PendingQuery => null;
        public bool ReconciliationRequired => false;
    }
}
