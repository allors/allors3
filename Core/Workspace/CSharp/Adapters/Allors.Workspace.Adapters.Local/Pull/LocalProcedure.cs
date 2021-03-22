// <copyright file="LocalPullInstantiate.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>
// <summary>Defines the ISessionExtension type.</summary>

namespace Allors.Workspace.Adapters.Local
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Database;
    using Database.Data;
    using Database.Derivations;
    using Database.Security;
    using IComposite = Database.Meta.IComposite;
    using Pull = Database.Data.Pull;

    public class LocalProcedure : IProcedureContext
    {
        private readonly Database.Data.Procedure procedure;
        private readonly IPreparedSelects preparedSelects;

        private List<IDerivationResult> errors;

        public LocalProcedure(ITransaction transaction, Database.Data.Procedure procedure, IAccessControlLists acls, IPreparedSelects preparedSelects)
        {
            this.Transaction = transaction;
            this.procedure = procedure;
            this.AccessControlLists = acls;
            this.preparedSelects = preparedSelects;
        }

        public ITransaction Transaction { get; }

        public IAccessControlLists AccessControlLists { get; }

        public void AddError(IDerivationResult derivationResult)
        {
            this.errors ??= new List<IDerivationResult>();
            this.errors.Add(derivationResult);
        }

        public void Execute(LocalPullResult pullResponse)
        {
            if (this.procedure.VersionByObject != null)
            {
                foreach (var kvp in procedure.VersionByObject)
                {
                    var @object = kvp.Key;
                    var version = kvp.Value;

                    if (!@object.Strategy.ObjectVersion.Equals(version))
                    {
                        pullResponse.AddVersionError(@object);
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
                foreach (var error in this.errors)
                {
                    pullResponse.AddDerivationErrors(error);
                }
            }
        }
    }
}
