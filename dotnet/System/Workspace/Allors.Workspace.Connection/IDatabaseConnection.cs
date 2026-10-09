// <copyright file="IDatabaseConnection.cs" company="Allors bv">
// Copyright (c) Allors bv. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Workspace.Connection
{
    using System;
    using System.Threading.Tasks;
    using Data;
    using Meta;
    using Ranges;

    /// <summary>
    /// The lowest layer of a workspace: the full contract between the workspace and the database,
    /// reads and writes alike, in ids, versions, meta types and role values, without objects.
    /// It pulls by the query model and keeps what it receives as records, grants, revocations and
    /// permissions; it pushes new and changed objects and invokes methods. The layers above build
    /// objects and change tracking on it; the transport underneath carries the wire.
    /// Make one call at a time: start the next call after the previous call's task completes.
    /// </summary>
    public interface IDatabaseConnection
    {
        /// <summary>
        /// The name of the workspace the server serves: it decides which classes and roles the
        /// records carry. Every request names it; a server that serves another workspace is
        /// refused.
        /// </summary>
        string WorkspaceName { get; }

        IMetaPopulation MetaPopulation { get; }

        /// <summary>
        /// The fingerprint of <see cref="MetaPopulation"/>. Every request names it; a server
        /// whose meta for the workspace has another fingerprint is refused.
        /// </summary>
        string MetaFingerprint { get; }

        IRanges<long> Ranges { get; }

        /// <summary>
        /// The id of the database the server serves, from the server's first response; null
        /// until then. A later response from another database faults the connection.
        /// </summary>
        string DatabaseId { get; }

        /// <summary>
        /// The id of the user the server serves this connection as, from the server's first
        /// response; null until then. A later response as another user faults the connection:
        /// every call throws, its records are cleared, and the user signs in with a new connection.
        /// </summary>
        long? UserId { get; }

        /// <summary>
        /// Raised after a pull for every record that the pull replaced by a newer one, once the
        /// records, grants and permissions of the pull are in.
        /// </summary>
        event EventHandler<RecordChangedEventArgs> RecordChanged;

        /// <summary>
        /// The record of the object, or null when the connection has not received it.
        /// </summary>
        IRecord GetRecord(long id);

        /// <summary>
        /// The id of the permission for the operation on the operand type of the class, or 0 when
        /// the connection has not received it.
        /// </summary>
        long GetPermission(IClass @class, IOperandType operandType, Operations operation);

        /// <summary>
        /// Pulls by the query model, with a procedure on the server when given, and brings the
        /// records of every object in the result up to date.
        /// </summary>
        Task<PullResult> PullAsync(Pull[] pulls, Procedure procedure = null);

        /// <summary>
        /// Pulls by a named route of the server with the arguments it takes, and brings the
        /// records of every object in the result up to date.
        /// </summary>
        Task<PullResult> PullAsync(string name, object args);

        /// <summary>
        /// Pushes new objects, each with a workspace id, and changed objects, each with the
        /// version it was changed from; null for none.
        /// </summary>
        Task<PushResult> PushAsync(PushNewObject[] newObjects, PushChangedObject[] changedObjects);

        Task<InvokeResult> InvokeAsync(Invocation[] invocations, InvokeOptions options = null);
    }
}
