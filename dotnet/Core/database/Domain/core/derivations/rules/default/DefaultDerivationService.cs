// <copyright file="DefaultDerivationFactory.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Database.Domain.Derivations.Rules.Default
{
    using Domain;

    public class DefaultDerivationService : IDerivationService
    {
        public DefaultDerivationService(Engine engine) => this.Engine = engine;

        public Engine Engine { get; }

        public int MaxCycles { get; set; } = 100;

        public IDerivation CreateDerivation(ITransaction transaction, bool continueOnError) => new DefaultDerivation(transaction, new Validation(), this.Engine, this.MaxCycles, false, continueOnError);
    }
}
