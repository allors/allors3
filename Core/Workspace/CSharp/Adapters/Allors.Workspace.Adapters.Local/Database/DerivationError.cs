// <copyright file="RemoteDerivationError.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Workspace.Adapters.Local
{
    using System.Collections.Generic;
    using System.Linq;
    using Meta;

    public class DerivationError : IDerivationError
    {
        private readonly Session session;
        private readonly Database.Derivations.IDerivationError derivationError;

        public DerivationError(Session session, Database.Derivations.IDerivationError derivationError)
        {
            this.session = session;
            this.derivationError = derivationError;
        }

        public IEnumerable<Role> Roles => this.derivationError.Relations
            .Select(v =>
                new Role(
                    this.session.Get<IObject>(v.Association.Id),
                    (IRelationType)this.session.Workspace.MetaPopulation.FindByTag(v.RelationType.Tag)));

        public string Message => this.derivationError.Message;
    }
}
