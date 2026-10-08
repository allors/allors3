// <copyright file="Result.cs" company="Allors bv">
// Copyright (c) Allors bv. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Workspace.Connection
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Allors.Protocol.Json.Api;
    using Meta;

    /// <summary>
    /// What the server answered to a pull, a push or an invoke: the errors, in ids.
    /// </summary>
    public abstract class Result
    {
        private readonly Response response;
        private readonly IMetaPopulation metaPopulation;

        private DerivationError[] derivationErrors;

        private protected Result(IMetaPopulation metaPopulation, Response response)
        {
            this.metaPopulation = metaPopulation;
            this.response = response;
        }

        public bool HasErrors => this.response.HasErrors;

        public string ErrorMessage => this.response._e;

        /// <summary>
        /// The ids of the objects whose version differed from the server's.
        /// </summary>
        public IReadOnlyList<long> VersionErrors => this.response._v ?? Array.Empty<long>();

        /// <summary>
        /// The ids of the objects the user may not access as requested.
        /// </summary>
        public IReadOnlyList<long> AccessErrors => this.response._a ?? Array.Empty<long>();

        /// <summary>
        /// The ids of the objects the server does not have.
        /// </summary>
        public IReadOnlyList<long> MissingErrors => this.response._m ?? Array.Empty<long>();

        public IReadOnlyList<DerivationError> DerivationErrors =>
            this.derivationErrors ??= this.response._d?.Select(v => new DerivationError(this.metaPopulation, v)).ToArray() ?? Array.Empty<DerivationError>();
    }
}
