// <copyright file="RemoteDerivationError.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Workspace.Adapters.Remote
{
    using System.Collections.Generic;
    using System.Linq;
    using Allors.Protocol.Json.Api;
    using Meta;

    public class DerivationError : IDerivationError
    {
        private readonly ResponseDerivationError responseDerivationError;
        private readonly ISession session;

        public DerivationError(ISession session, ResponseDerivationError responseDerivationError)
        {
            this.session = session;
            this.responseDerivationError = responseDerivationError;
        }

        public string Message => this.responseDerivationError.e;

        public IEnumerable<Role> Roles =>
            from r in this.responseDerivationError.r
            let association = this.session.GetOne<IObject>(r[0])
            let relationType = (IRelationType)this.session.Workspace.DatabaseConnection.Configuration.MetaPopulation.FindByTag((int)r[1])
            select new Role(association, relationType);
    }
}
