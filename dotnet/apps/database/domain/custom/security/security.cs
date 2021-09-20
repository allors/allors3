// <copyright file="Security.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Database.Domain
{
    using Meta;

    public partial class Security
    {
        public void Grantemployee(ObjectType objectType, params Operations[] operations) => this.Grant(Roles.EmployeeId, objectType, operations);

        private void CustomOnPreSetup()
        {
            // Default access policy
            var security = new Security(this.transaction);

            var full = new[] { Operations.Read, Operations.Write, Operations.Execute };

            foreach (ObjectType @class in this.transaction.Database.MetaPopulation.DatabaseClasses)
            {
                security.GrantAdministrator(@class, full);
                security.Grantemployee(@class, Operations.Read);
                security.GrantCreator(@class, full);
            }
        }

        private void CustomOnPostSetup()
        {
        }
    }
}
