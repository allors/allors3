// <copyright file="ServicesTest.cs" company="Allors bv">
// Copyright (c) Allors bv. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Database.Adapters.Memory
{
    using System;
    using Adapters;

    public class ServicesTest : Adapters.ServicesTest, IDisposable
    {
        private readonly Profile profile = new Profile();

        protected override IProfile Profile => this.profile;

        public override void Dispose() => this.profile.Dispose();

        protected override void SwitchDatabase()
        {
        }

        protected override IDatabase CreatePopulation() => this.profile.CreateDatabase();

        protected override ITransaction CreateTransaction() => this.profile.CreateTransaction();
    }
}
