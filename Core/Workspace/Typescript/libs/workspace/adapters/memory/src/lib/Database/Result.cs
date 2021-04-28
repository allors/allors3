// <copyright file="RemoteResult.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Workspace.Adapters.Remote
{
    using System.Collections.Generic;
    using System.Linq;
    using Allors.Protocol.Json.Api;

    public abstract class Result : IResult
    {
        private readonly ISession session;
        private readonly Response response;

        private IDerivationError[] derivationErrors;

        protected Result(ISession session, Response response)
        {
            this.session = session;
            this.response = response;
        }

        public bool HasErrors => this.response.HasErrors;

        public string ErrorMessage => this.response.ErrorMessage;

        public IEnumerable<IObject> VersionErrors => this.session.Get<IObject>(this.response.VersionErrors);

        public IEnumerable<IObject> AccessErrors => this.session.Get<IObject>(this.response.AccessErrors);

        public IEnumerable<IObject> MissingErrors => this.session.Get<IObject>(this.response.MissingErrors);

        public IEnumerable<IDerivationError> DerivationErrors
        {
            get
            {
                if (this.derivationErrors != null)
                {
                    return this.derivationErrors;
                }

                if (this.response.DerivationErrors?.Length > 0)
                {
                    return this.derivationErrors ??= this.response.DerivationErrors
                        .Select(v => (IDerivationError)new DerivationError(this.session, v)).ToArray();
                }

                return this.derivationErrors;
            }
        }
    }
}
