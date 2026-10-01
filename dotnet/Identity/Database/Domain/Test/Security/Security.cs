// <copyright file="Security.cs" company="Allors bv">
// Copyright (c) Allors bv. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Database.Domain
{
    using Meta;

    public partial class Security
    {
        private void TestOnPreSetup()
        {
        }

        private void TestOnPostSetup()
        {
            foreach (ObjectType @class in this.transaction.Database.MetaPopulation.DatabaseClasses)
            {
                this.GrantAdministrator(@class, Operations.Read, Operations.Write, Operations.Execute);
            }
        }
    }
}
