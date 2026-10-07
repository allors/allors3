// <copyright file="ObjectsBase.v.cs" company="Allors bv">
// Copyright (c) Allors bv. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Database.Domain
{
    // The dispatch of each phase to the hooks of the domains, base first: the order of
    // MetaPopulation.SortedDomains reversed, which SetupOrderTests checks.
    public abstract partial class ObjectsBase<T> where T : IObject
    {
        public void Prepare(Setup setup)
        {
            this.CorePrepare(setup);
            this.Level1Prepare(setup);
            this.Level2Prepare(setup);
            this.Plugin1Prepare(setup);
            this.TestPrepare(setup);
        }

        public void Setup(Setup setup)
        {
            this.CoreSetup(setup);
            this.Level1Setup(setup);
            this.Level2Setup(setup);
            this.Plugin1Setup(setup);
            this.TestSetup(setup);

            this.Transaction.Derive();
        }

        public void Prepare(Security security)
        {
            this.CorePrepare(security);
            this.Level1Prepare(security);
            this.Level2Prepare(security);
            this.Plugin1Prepare(security);
            this.TestPrepare(security);
        }

        public void Secure(Security security)
        {
            this.CoreSecure(security);
            this.Level1Secure(security);
            this.Level2Secure(security);
            this.Plugin1Secure(security);
            this.TestSecure(security);
        }
    }
}
