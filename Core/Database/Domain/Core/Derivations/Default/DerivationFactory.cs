// <copyright file="DerivationFactory.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Database.Domain.Derivations.Default
{
    using Derivations;
    using Domain;

    public class DerivationFactory : IDerivationFactory
    {
        public DerivationFactory(Engine engine) => this.Engine = engine;

        public Engine Engine { get; }

        public int MaxCycles { get; set; } = 10;

        public IDerivation CreateDerivation(ITransaction transaction) => new DefaultDerivation(transaction, this.Engine, this.MaxCycles);
    }
}
