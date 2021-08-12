// <copyright file="DefaultDerivationFactory.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Database.Configuration.Derivations.Default
{
    using Database.Derivations;

    public class DerivationService : IDerivationService
    {
        public DerivationService(Engine engine) => this.Engine = engine;

        public Engine Engine { get; }

        public int MaxCycles { get; set; } = 100;

        public IDerivation CreateDerivation(ITransaction transaction, bool continueOnError) => new Derivation(transaction, new Validation(), this.Engine, this.MaxCycles, false, continueOnError);
    }
}
