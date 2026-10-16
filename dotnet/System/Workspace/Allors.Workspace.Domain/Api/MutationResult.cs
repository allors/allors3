// <copyright file="MutationResult.cs" company="Allors bv">
// Copyright (c) Allors bv. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Workspace
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    public enum MutationOutcome
    {
        Committed,
        Rejected,
        Unknown,
    }

    public enum MutationReadStatus
    {
        NotRequested,
        Unavailable,
        Succeeded,
        Failed,
    }

    public enum MutationAcknowledgementStatus
    {
        NotApplicable,
        Applied,
        Failed,
    }

    public enum MutationRefreshApplicationStatus
    {
        NotApplicable,
        Skipped,
        Applied,
        Conflicted,
        Failed,
    }

    public enum MutationApplicationStage
    {
        Acknowledgement,
        Refresh,
        Discovery,
        Notification,
    }

    /// <summary>Owned diagnostic values remain inspectable without loading workspace objects.</summary>
    public sealed class MutationDiagnostics
    {
        public MutationDiagnostics(string errorMessage = null, IEnumerable<MutationDiagnostic> errors = null,
            IEnumerable<long> versionErrorIds = null, IEnumerable<long> accessErrorIds = null,
            IEnumerable<long> missingErrorIds = null, IEnumerable<MutationDerivationDiagnostic> derivationErrors = null)
        {
            this.ErrorMessage = errorMessage;
            this.Errors = MutationValues.Copy(errors);
            this.VersionErrorIds = MutationValues.Copy(versionErrorIds);
            this.AccessErrorIds = MutationValues.Copy(accessErrorIds);
            this.MissingErrorIds = MutationValues.Copy(missingErrorIds);
            this.DerivationErrors = MutationValues.Copy(derivationErrors);
        }

        public string ErrorMessage { get; }

        public IReadOnlyList<MutationDiagnostic> Errors { get; }

        public IReadOnlyList<long> VersionErrorIds { get; }

        public IReadOnlyList<long> AccessErrorIds { get; }

        public IReadOnlyList<long> MissingErrorIds { get; }

        public IReadOnlyList<MutationDerivationDiagnostic> DerivationErrors { get; }
    }

    public sealed class MutationDiagnostic
    {
        public MutationDiagnostic(string code, string message, IEnumerable<long> objectIds = null, int? selectionIndex = null, Exception cause = null)
        {
            this.Code = code;
            this.Message = message;
            this.ObjectIds = MutationValues.Copy(objectIds);
            this.SelectionIndex = selectionIndex;
            this.Cause = cause;
        }

        public string Code { get; }

        public string Message { get; }

        public IReadOnlyList<long> ObjectIds { get; }

        public int? SelectionIndex { get; }

        public Exception Cause { get; }
    }

    public sealed class MutationDerivationDiagnostic
    {
        public MutationDerivationDiagnostic(string message, IEnumerable<MutationDerivationRelation> relations)
        {
            this.Message = message;
            this.Relations = MutationValues.Copy(relations);
        }

        public string Message { get; }

        public IReadOnlyList<MutationDerivationRelation> Relations { get; }
    }

    public sealed class MutationDerivationRelation
    {
        public MutationDerivationRelation(long objectId, string relationTypeTag)
        {
            this.ObjectId = objectId;
            this.RelationTypeTag = relationTypeTag;
        }

        public long ObjectId { get; }

        public string RelationTypeTag { get; }
    }

    public sealed class CreationMapping
    {
        public CreationMapping(long workspaceId, long databaseId)
        {
            this.WorkspaceId = workspaceId;
            this.DatabaseId = databaseId;
        }

        public long WorkspaceId { get; }

        public long DatabaseId { get; }
    }

    public sealed class MutationRefresh
    {
        public MutationRefresh(MutationReadStatus status, IPullResult result = null, IEnumerable<MutationDiagnostic> errors = null)
        {
            this.Status = status;
            this.Result = result;
            this.Errors = MutationValues.Copy(errors);
        }

        public MutationReadStatus Status { get; }

        public IPullResult Result { get; }

        public IReadOnlyList<MutationDiagnostic> Errors { get; }
    }

    public sealed class MutationApplication
    {
        public MutationApplication(MutationAcknowledgementStatus acknowledgement, MutationRefreshApplicationStatus refresh,
            IEnumerable<MutationApplicationError> errors = null)
        {
            this.Acknowledgement = acknowledgement;
            this.Refresh = refresh;
            this.Errors = MutationValues.Copy(errors);
        }

        public MutationAcknowledgementStatus Acknowledgement { get; }

        public MutationRefreshApplicationStatus Refresh { get; }

        public IReadOnlyList<MutationApplicationError> Errors { get; }
    }

    public sealed class MutationApplicationError
    {
        public MutationApplicationError(MutationApplicationStage stage, string message, Exception cause = null)
        {
            this.Stage = stage;
            this.Message = message;
            this.Cause = cause;
        }

        public MutationApplicationStage Stage { get; }

        public string Message { get; }

        public Exception Cause { get; }
    }

    public sealed class PushDiscovery
    {
        public PushDiscovery(MutationReadStatus status, IEnumerable<MutationDiagnostic> errors = null)
        {
            this.Status = status;
            this.Errors = MutationValues.Copy(errors);
        }

        public MutationReadStatus Status { get; }

        public IReadOnlyList<MutationDiagnostic> Errors { get; }
    }

    internal static class MutationValues
    {
        internal static IReadOnlyList<T> Copy<T>(IEnumerable<T> values) => Array.AsReadOnly(values?.ToArray() ?? Array.Empty<T>());
    }
}
