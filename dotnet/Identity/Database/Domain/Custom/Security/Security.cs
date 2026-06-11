// <copyright file="Security.cs" company="Allors bv">
// Copyright (c) Allors bv. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Database.Domain
{
    using Meta;

    public partial class Security
    {
        private void CustomOnPreSetup()
        {
        }

        private void CustomOnPostSetup()
        {
            // Default access policy
            var security = new Security(this.transaction);

            var full = new[] { Operations.Read, Operations.Write, Operations.Execute };

            foreach (ObjectType @class in this.transaction.Database.MetaPopulation.DatabaseClasses)
            {
                security.GrantAdministrator(@class, full);
                security.GrantCreator(@class, full);
                security.GrantGuest(@class, Operations.Read);
            }
        }
    }
}
