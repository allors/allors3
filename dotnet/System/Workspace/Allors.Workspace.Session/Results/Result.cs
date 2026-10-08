// <copyright file="Result.cs" company="Allors bv">
// Copyright (c) Allors bv. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Workspace.Session
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using ConnectionResult = Allors.Workspace.Connection.Result;

    /// <summary>
    /// A result of the connection with its ids turned into the objects of a session.
    /// </summary>
    public abstract class Result : IResult
    {
        private readonly ConnectionResult result;

        private IDerivationError[] derivationErrors;

        private IList<IObject> mergeErrors;

        protected Result(ISession session, ConnectionResult result)
        {
            this.Session = session;
            this.result = result;
        }

        public ISession Session { get; }

        public bool HasErrors => this.result.HasErrors || this.mergeErrors?.Count > 0;

        public string ErrorMessage => this.result.ErrorMessage;

        public IEnumerable<IObject> VersionErrors => this.Session.Instantiate<IObject>(this.result.VersionErrors);

        public IEnumerable<IObject> AccessErrors => this.Session.Instantiate<IObject>(this.result.AccessErrors);

        public IEnumerable<IObject> MissingErrors => this.Session.Instantiate<IObject>(this.result.MissingErrors);

        public IEnumerable<IDerivationError> DerivationErrors
        {
            get
            {
                if (this.derivationErrors != null)
                {
                    return this.derivationErrors;
                }

                if (this.result.DerivationErrors.Count > 0)
                {
                    return this.derivationErrors ??= this.result.DerivationErrors
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
