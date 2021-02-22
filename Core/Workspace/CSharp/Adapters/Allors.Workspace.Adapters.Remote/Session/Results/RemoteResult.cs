// <copyright file="RemoteResult.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Workspace.Adapters.Remote
{
    using System.Linq;
    using Allors.Protocol.Json.Api;

    public abstract class RemoteResult : IResult
    {
        private IDerivationError[] derivationErrors;

        protected RemoteResult(Response response) => this.Response = response;

        public Response Response { get; }

        public bool HasErrors => this.Response.HasErrors;

        public string ErrorMessage => this.Response.ErrorMessage;

        public string[] VersionErrors => this.Response.VersionErrors;

        public string[] AccessErrors => this.Response.AccessErrors;

        public string[] MissingErrors => this.Response.MissingErrors;

        public IDerivationError[] DerivationErrors => this.derivationErrors ??= this.Response.DerivationErrors.Select(v => (IDerivationError)new RemoteDerivationError(v)).ToArray();
    }
}
