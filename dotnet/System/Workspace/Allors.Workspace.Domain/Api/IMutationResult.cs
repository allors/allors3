// <copyright file="IMutationResult.cs" company="Allors bv">
// Copyright (c) Allors bv. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Workspace
{
    public interface IMutationResult
    {
        MutationOutcome Outcome { get; }
        MutationDiagnostics Diagnostics { get; }
        MutationRefresh Refresh { get; }
        MutationApplication Application { get; }
        bool ReconciliationRequired { get; }
    }
}
