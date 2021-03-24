// <copyright file="DefaultDerivationFactory.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Workspace.Configuration
{
    using Derivations;
    using Domain;

    public class DefaultDerivationFactory : IDerivationFactory
    {
        public IRule[] Rules { get; }

        public int MaxCycles { get; set; } = 10;

        public DefaultDerivationFactory(IRule[] rules) => this.Rules = rules;

        public IDerivation CreateDerivation(ISession session) => new Derivations.Default.Derivation(session, this.Rules, this.MaxCycles);
    }
}
