// <copyright file="LocalPullResult.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Workspace.Adapters.Local
{
    using System.Collections.Generic;
    using System.Linq;
    using IDerivationError = Allors.Workspace.IDerivationError;

    public abstract class Result : IInvokeResult
    {
        private List<Strategy> accessErrorStrategies;
        private List<long> databaseMissingIds;
        private List<long> versionErrors;
        private List<Database.Derivations.IDerivationError> derivationErrors;

        protected Result(Session session)
        {
            this.Session = session;
            this.accessErrorStrategies = new List<Strategy>();
            this.databaseMissingIds = new List<long>();
            this.versionErrors = new List<long>();
        }

        public string ErrorMessage { get; protected set; }

        public IEnumerable<IObject> VersionErrors => this.versionErrors?.Select(v => this.Session.Get<IObject>(v));

        public IEnumerable<IObject> AccessErrors => this.accessErrorStrategies?.Select(v => v.Object);

        public IEnumerable<IObject> MissingErrors => this.Session.Get<IObject>(this.databaseMissingIds);

        public IEnumerable<IDerivationError> DerivationErrors => this.derivationErrors?.Select(v => (IDerivationError)new DerivationError(this.Session, v)).ToArray();

        public bool HasErrors => !string.IsNullOrWhiteSpace(this.ErrorMessage) || this.accessErrorStrategies?.Count > 0 || this.databaseMissingIds?.Count > 0 || this.versionErrors?.Count > 0 || this.derivationErrors?.Count > 0;

        internal void AddDerivationErrors(Database.Derivations.IDerivationError[] errors) => this.derivationErrors.AddRange(errors);

        internal void AddMissingId(long id) => this.databaseMissingIds.Add(id);

        internal void AddAccessError(Strategy strategy) => this.accessErrorStrategies.Add(strategy);

        internal void AddVersionError(long id) => this.versionErrors.Add(id);

        protected Session Session { get; }
    }
}
