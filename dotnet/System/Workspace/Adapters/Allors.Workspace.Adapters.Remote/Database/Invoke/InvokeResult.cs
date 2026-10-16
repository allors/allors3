// <copyright file="RemoteInvokeResult.cs" company="Allors bv">
// Copyright (c) Allors bv. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Workspace.Adapters.Remote
{
    using Allors.Protocol.Json.Api.Invoke;

    public class InvokeResult : Result, IInvokeResult
    {
        public InvokeResult(ISession session, InvokeResponse invokeResponse) : base(session, invokeResponse)
        {
            this.Outcome = invokeResponse.HasErrors ? MutationOutcome.Rejected : MutationOutcome.Committed;
            this.Diagnostics = ScopedPushResult.CaptureDiagnostics(invokeResponse);
        }

        public MutationOutcome Outcome { get; }
        public MutationDiagnostics Diagnostics { get; }
        public MutationRefresh Refresh { get; } = new MutationRefresh(MutationReadStatus.NotRequested);
        public MutationApplication Application { get; } = new MutationApplication(MutationAcknowledgementStatus.NotApplicable, MutationRefreshApplicationStatus.NotApplicable);
        public bool ReconciliationRequired => false;
    }
}
