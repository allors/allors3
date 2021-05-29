// <copyright file="DefaultDerivationFactory.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Database.Domain.Derivations.Default
{
    using Domain;

    public class DefaultDerivationFactory : IDerivationFactory
    {
        public DefaultDerivationFactory(Engine engine) => this.Engine = engine;

        public Engine Engine { get; }

        public int MaxCycles { get; set; } = 10;

        public IDerivation CreateDerivation(ITransaction transaction) => new DefaultDerivation(transaction, new Validation(), this.Engine, this.MaxCycles);
    }
}
