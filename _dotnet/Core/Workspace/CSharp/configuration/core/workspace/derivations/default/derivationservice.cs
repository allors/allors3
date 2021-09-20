// <copyright file="DerivationService.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Workspace.Configuration.Derivations.Default
{
    using Allors.Workspace.Derivations;

    public class DerivationService : IDerivationService
    {
        public DerivationService(Engine engine) => this.Engine = engine;

        public Engine Engine { get; }

        public int MaxCycles { get; set; } = 10;

        public IDerivation CreateDerivation(ISession session) => new Derivation(session, this.Engine, this.MaxCycles);
    }
}
