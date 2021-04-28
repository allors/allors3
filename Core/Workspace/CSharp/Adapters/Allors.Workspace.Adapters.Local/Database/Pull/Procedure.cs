// <copyright file="LocalPullInstantiate.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>
// <summary>Defines the ISessionExtension type.</summary>

namespace Allors.Workspace.Adapters.Local
{
    using System;
    using System.Collections.Generic;
    using Database;
    using Database.Data;
    using Database.Derivations;
    using Database.Security;

    public class Procedure : IProcedureContext
    {
        private readonly Database.Data.Procedure procedure;

        private List<IDerivationResult> errors;

        public Procedure(ITransaction transaction, Database.Data.Procedure procedure, IAccessControlLists acls)
        {
            this.Transaction = transaction;
            this.procedure = procedure;
            this.AccessControlLists = acls;
        }

        public ITransaction Transaction { get; }

        public IAccessControlLists AccessControlLists { get; }

        public void AddError(IDerivationResult derivationResult)
        {
            this.errors ??= new List<IDerivationResult>();
            this.errors.Add(derivationResult);
        }

        public void Execute(PullResult pullResponse)
        {
            if (this.procedure.Pool != null)
            {
                foreach (var kvp in this.procedure.Pool)
                {
                    var @object = kvp.Key;
                    var version = kvp.Value;

                    if (!@object.Strategy.ObjectVersion.Equals(version))
                    {
                        pullResponse.AddVersionError(@object.Id);
                    }
                }

                if (pullResponse.HasErrors)
                {
                    return;
                }
            }

            var proc = this.Transaction.Database.Procedures.Get(this.procedure.Name);
            if (proc == null)
            {
                throw new Exception($"Missing procedure {this.procedure.Name}");
            }

            var input = new ProcedureInput(this.Transaction.Database.ObjectFactory, this.procedure);

            proc.Execute(this, input, pullResponse);

            if (this.errors?.Count > 0)
            {
                foreach (var derivationResult in this.errors)
                {
                    pullResponse.AddDerivationErrors(derivationResult.Errors);
                }
            }
        }
    }
}
