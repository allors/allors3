// <copyright file="DerivationError.cs" company="Allors bv">
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
    /// An error a derivation on the server raised: its message and the roles it names.
    /// </summary>
    public sealed class DerivationError
    {
        internal DerivationError(IMetaPopulation metaPopulation, ResponseDerivationError error)
        {
            this.Message = error.m;
            this.Roles = error.r?.Select(v => new DerivationRole(v.i, (IRelationType)metaPopulation.FindByTag(v.r))).ToArray() ?? Array.Empty<DerivationRole>();
        }

        public string Message { get; }

        public IReadOnlyList<DerivationRole> Roles { get; }
    }
}
