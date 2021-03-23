// <copyright file="RemoteDerivationError.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Workspace.Adapters.Local
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Database.Derivations;
    using Meta;
    using IDerivationError = Workspace.IDerivationError;

    public class LocalDerivationError : IDerivationError
    {
        private readonly ISession session;
        private readonly IDerivationResult derivationResult;

        public LocalDerivationError(ISession session, IDerivationResult derivationResult)
        {
            this.session = session;
            this.derivationResult = derivationResult;
        }

        public string ErrorMessage => "TODO"; // TODO: ?

        public IEnumerable<Role> Roles =>
            from error in this.derivationResult.Errors
            from r in error.Relations
            let association = this.session.Get<IObject>(r.Association.Id)
            let relationType = (IRelationType)this.session.Workspace.MetaPopulation.Find(r.RelationType.Id)
            select new Role(association, relationType);
    }
}
