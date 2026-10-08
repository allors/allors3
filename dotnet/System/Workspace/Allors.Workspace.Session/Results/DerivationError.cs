// <copyright file="DerivationError.cs" company="Allors bv">
// Copyright (c) Allors bv. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Workspace.Session
{
    using System.Collections.Generic;
    using System.Linq;
    using ConnectionDerivationError = Allors.Workspace.Connection.DerivationError;

    public sealed class DerivationError : IDerivationError
    {
        private readonly ISession session;
        private readonly ConnectionDerivationError error;

        internal DerivationError(ISession session, ConnectionDerivationError error)
        {
            this.session = session;
            this.error = error;
        }

        public string Message => this.error.Message;

        public IEnumerable<Role> Roles => this.error.Roles.Select(v => new Role(this.session.Instantiate<IObject>(v.ObjectId), v.RelationType));
    }
}
