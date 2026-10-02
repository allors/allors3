// <copyright file="TestDatabaseServices.cs" company="Allors bv">
// Copyright (c) Allors bv. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Database.Configuration
{
    using Database.Derivations;
    using Derivations.Default;

    public class TestDatabaseServices : DatabaseServices
    {
        public TestDatabaseServices(Engine engine) : base(engine) { }

        protected override IDerivationService CreateDerivationFactory() => new DerivationService(this.Engine);
    }
}
