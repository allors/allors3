// <copyright file="RemoteDerivationError.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Workspace.Adapters.Remote
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Allors.Protocol.Json.Api;
    using Meta;

    public class RemoteDerivationError : IDerivationError
    {
        private readonly ISession session;
        private readonly ResponseDerivationError responseDerivationError;

        public RemoteDerivationError(ISession session, ResponseDerivationError responseDerivationError)
        {
            this.session = session;
            this.responseDerivationError = responseDerivationError;
        }

        public string ErrorMessage => this.responseDerivationError.M;

        public IEnumerable<Role> Roles =>
            from r in this.responseDerivationError.R
            let association = this.session.Get<IObject>(long.Parse(r[0]))
            let relationType = (IRelationType)this.session.Workspace.MetaPopulation.Find(Guid.Parse(r[1]))
            select new Role(association, relationType);
    }
}
