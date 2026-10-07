// <copyright file="Security.cs" company="Allors bv">
// Copyright (c) Allors bv. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Database.Domain
{
    using Meta;

    // The hooks of the concrete domain in the security setup of a database: each records that it
    // ran, and the last one grants the administrators every operation on every class.
    public partial class Security
    {
        private void TestOnPreSetup() => this.TestLog.Record("Test", "Security.OnPreSetup");

        private void TestOnPostSetup()
        {
            this.TestLog.Record("Test", "Security.OnPostSetup");

            foreach (ObjectType @class in this.transaction.Database.MetaPopulation.DatabaseClasses)
            {
                this.GrantAdministrator(@class, Operations.Read, Operations.Write, Operations.Execute);
            }
        }

        private ILevel1Log TestLog => this.transaction.Database.Services.Get<ILevel1Log>();
    }
}
