// <copyright file="Parties.cs" company="Allors bv">
// Copyright (c) Allors bv. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Database.Domain
{
    public partial class Parties
    {
        public static void Daily(ITransaction transaction)
        {
            foreach (Party party in new Parties(transaction).Extent())
            {
                party.DeriveRelationships();
            }
        }

        protected override void AppsPrepare(Setup setup)
        {
            setup.AddDependency(this.ObjectType, this.M.ContactMechanismPurpose);
            setup.AddDependency(this.ObjectType, this.M.Settings);
        }
    }
}
