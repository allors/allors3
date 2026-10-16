// <copyright file="InvokeResponseBuilder.cs" company="Allors bv">
// Copyright (c) Allors bv. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Database.Protocol.Json
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;
    using Meta;
    using Allors.Protocol.Json.Api.Invoke;
    using Derivations;
    using Security;
    using Services;

    public class InvokeResponseBuilder
    {
        private const long MaximumInteger = 9007199254740991;
        private const string UnavailableMethod = "The requested method is not available for this object in the selected workspace.";
        private readonly ITransaction transaction;
        private readonly Func<IValidation> derive;
        private readonly IAccessControl accessControl;
        private readonly ISet<IClass> allowedClasses;

        public InvokeResponseBuilder(ITransaction transaction, Func<IValidation> derive, IAccessControl accessControl, ISet<IClass> allowedClasses)
        {
            if (transaction.Database is not Adapters.Sql.Database)
            {
                throw new NotSupportedException("Workspace operations require PostgreSQL or SQL Server. Memory remains available for database and domain tests.");
            }

            this.transaction = transaction;
            this.derive = derive;
            this.accessControl = accessControl;
            this.allowedClasses = allowedClasses;
        }

        /// <summary>Prepare the whole ordered batch before any method can have effects.</summary>
        internal PreparedInvocation[] Prepare(InvokeRequest request, string workspaceName, out InvokeResponse response)
        {
            ArgumentNullException.ThrowIfNull(request);
            if (request.l == null) throw new ArgumentException("Supply an invocation array, including an empty array for an atomic no-op.", nameof(request));
            response = new InvokeResponse();
            var versions = new Dictionary<long, long>();
            var prepared = new List<PreparedInvocation>();
            foreach (var invocation in request.l)
            {
                if (invocation == null || invocation.i < 1 || invocation.i > MaximumInteger || invocation.v < 1 || invocation.v > MaximumInteger || string.IsNullOrWhiteSpace(invocation.m))
                {
                    throw new ArgumentException("Supply a positive safe target ID and original version, and a nonblank method tag for every invocation.", nameof(request));
                }

                if (versions.TryGetValue(invocation.i, out var previous) && previous != invocation.v)
                {
                    throw new ArgumentException("Use the same original version for every invocation of one target.", nameof(request));
                }

                versions[invocation.i] = invocation.v;
                var value = this.transaction.Instantiate(invocation.i);
                if (value == null || value.Strategy.IsDeleted) { response.AddMissingError(invocation.i); return Array.Empty<PreparedInvocation>(); }
                if (this.allowedClasses?.Contains(value.Strategy.Class) != true) { response.AddAccessError(value.Id); return Array.Empty<PreparedInvocation>(); }
                var method = ((IComposite)value.Strategy.Class).MethodTypes.FirstOrDefault(item => item.Tag == invocation.m);
                if (method == null) { response._e = UnavailableMethod; return Array.Empty<PreparedInvocation>(); }
                if (!method.WorkspaceNames.Contains(workspaceName, StringComparer.Ordinal)) { response.AddAccessError(value.Id); return Array.Empty<PreparedInvocation>(); }
                if (invocation.v != value.Strategy.ObjectVersion) { response.AddVersionError(value); return Array.Empty<PreparedInvocation>(); }
                if (!this.accessControl[value].CanExecute(method)) { response.AddAccessError(value.Id); return Array.Empty<PreparedInvocation>(); }
                MethodInfo binding;
                try
                {
                    binding = value.GetType().GetMethod(method.Name, BindingFlags.Public | BindingFlags.Instance, null, Type.EmptyTypes, null);
                }
                catch (Exception exception)
                {
                    System.Diagnostics.Trace.TraceError("Invoke method binding failed: {0}", exception);
                    response._e = UnavailableMethod;
                    return Array.Empty<PreparedInvocation>();
                }

                if (binding == null)
                {
                    System.Diagnostics.Trace.TraceError("No public zero-argument binding exists for Invoke method {0} on {1}.", method.Name, value.GetType());
                    response._e = UnavailableMethod;
                    return Array.Empty<PreparedInvocation>();
                }
                prepared.Add(new PreparedInvocation(value, method, binding, workspaceName));
            }

            return prepared.ToArray();
        }

        /// <summary>Apply without transaction boundaries; the caller owns atomic commit or rollback.</summary>
        internal void Apply(PreparedInvocation[] prepared, InvokeResponse response, Action ensureValid)
        {
            foreach (var invocation in prepared)
            {
                ensureValid();
                var value = invocation.Target;
                if (value.Strategy.IsDeleted || !ReferenceEquals(this.transaction.Instantiate(value.Id), value)) { response.AddMissingError(value.Id); return; }
                if (this.allowedClasses?.Contains(value.Strategy.Class) != true || !invocation.Method.WorkspaceNames.Contains(invocation.WorkspaceName, StringComparer.Ordinal))
                {
                    response.AddAccessError(value.Id);
                    return;
                }

                // Workspace ACLs cache each object. A preceding method or derivation may have
                // changed permission, so obtain a fresh transaction-current ACL for every entry.
                var currentAccess = this.transaction.Services.Get<IWorkspaceAclsService>().Create(invocation.WorkspaceName);
                if (!currentAccess[value].CanExecute(invocation.Method)) { response.AddAccessError(value.Id); return; }
                ensureValid();
                try
                {
                    invocation.Binding.Invoke(value, null);
                }
                catch (Exception exception)
                {
                    while (exception.InnerException != null) exception = exception.InnerException;
                    System.Diagnostics.Trace.TraceError("Invoke method execution failed: {0}", exception);
                    response._e = string.IsNullOrWhiteSpace(exception.Message)
                        ? "The invoked method failed. Inspect the server diagnostics before trying a new batch."
                        : exception.Message;
                    return;
                }

                ensureValid();
                var validation = this.derive();
                ensureValid();
                if (validation.HasErrors)
                {
                    response.AddDerivationErrors(validation);
                    if (!response.HasErrors)
                    {
                        response._e = "The invocation derivation failed. Inspect the server diagnostics and correct the validation failure before trying a new batch.";
                    }
                    return;
                }
            }
        }

        internal sealed class PreparedInvocation
        {
            internal PreparedInvocation(IObject target, IMethodType method, MethodInfo binding, string workspaceName)
            {
                this.Target = target; this.Method = method; this.Binding = binding; this.WorkspaceName = workspaceName;
            }

            internal IObject Target { get; }
            internal IMethodType Method { get; }
            internal MethodInfo Binding { get; }
            internal string WorkspaceName { get; }
        }
    }
}
