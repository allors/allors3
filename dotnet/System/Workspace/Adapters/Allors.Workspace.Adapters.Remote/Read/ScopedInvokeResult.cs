// <copyright file="ScopedInvokeResult.cs" company="Allors bv">
// Copyright (c) Allors bv. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Workspace.Adapters.Remote
{
    using System;
    using System.Collections.Generic;

    /// <summary>A detached method outcome. Invoking never acknowledges unsaved edits.</summary>
    internal sealed class ScopedInvokeResult : IInvokeResult
    {
        private readonly ScopedPushResult result;
        internal ScopedInvokeResult(Session session, MutationOutcome outcome, MutationDiagnostics diagnostics,
            MutationRefresh refresh, MutationApplication application, bool reconciliationRequired)
        {
            this.result = new ScopedPushResult(session, outcome, diagnostics, Array.Empty<CreationMapping>(), refresh,
                application, new PushDiscovery(MutationReadStatus.NotRequested), null, reconciliationRequired);
        }
        public MutationOutcome Outcome => this.result.Outcome;
        public MutationDiagnostics Diagnostics => this.result.Diagnostics;
        public MutationRefresh Refresh => this.result.Refresh;
        public MutationApplication Application => this.result.Application;
        public bool ReconciliationRequired => this.result.ReconciliationRequired;
        public bool HasErrors => this.result.HasErrors;
        public string ErrorMessage => this.result.ErrorMessage;
        public IEnumerable<IObject> VersionErrors => this.result.VersionErrors;
        public IEnumerable<IObject> AccessErrors => this.result.AccessErrors;
        public IEnumerable<IObject> MissingErrors => this.result.MissingErrors;
        public IEnumerable<IDerivationError> DerivationErrors => this.result.DerivationErrors;
    }
}
