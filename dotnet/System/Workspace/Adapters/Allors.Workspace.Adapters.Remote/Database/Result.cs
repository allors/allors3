// <copyright file="RemoteResult.cs" company="Allors bv">
// Copyright (c) Allors bv. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Workspace.Adapters.Remote
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Allors.Protocol.Json.Api;

    public abstract class Result : IResult
    {
        private readonly Response response;

        private IDerivationError[] derivationErrors;

        private IList<IObject> mergeErrors;

        protected Result(ISession session, Response response)
        {
            this.Session = session;
            this.response = response;
        }

        public ISession Session { get; }

        public bool HasErrors => this.response.HasErrors || this.mergeErrors?.Count > 0;

        public string ErrorMessage => this.response._e;

        public IEnumerable<IObject> VersionErrors => this.response._v != null ? this.Session.Instantiate<IObject>(this.response._v) : Array.Empty<IObject>();

        public IEnumerable<IObject> AccessErrors => this.response._a != null ? this.Session.Instantiate<IObject>(this.response._a) : Array.Empty<IObject>();

        public IEnumerable<IObject> MissingErrors => this.response._m != null ? this.Session.Instantiate<IObject>(this.response._m) : Array.Empty<IObject>();

        public IEnumerable<IDerivationError> DerivationErrors
        {
            get
            {
                if (this.derivationErrors != null)
                {
                    return this.derivationErrors;
                }

                if (this.response._d?.Length > 0)
                {
                    return this.derivationErrors ??= this.response._d
                        .Select(v => (IDerivationError)new DerivationError(this.Session, v)).ToArray();
                }

                return this.derivationErrors;
            }
        }

        public IEnumerable<IObject> MergeErrors => this.mergeErrors ?? Array.Empty<IObject>();

        public void AddMergeError(IObject @object)
        {
            this.mergeErrors ??= new List<IObject>();
            this.mergeErrors.Add(@object);
        }
    }
}
