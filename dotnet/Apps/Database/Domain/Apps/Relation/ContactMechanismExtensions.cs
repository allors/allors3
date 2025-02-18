// <copyright file="ContactMechanismExtensions.cs" company="Allors bv">
// Copyright (c) Allors bv. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Database.Domain
{
    public static partial class ContactMechanismExtensions
    {
        public static void AppsDelete(this ContactMechanism @this, DeletableDelete method)
        {
            foreach(PartyContactMechanism partyContactMechanism in @this.PartyContactMechanismsWhereContactMechanism)
            {
                partyContactMechanism.CascadingDelete();
            }
        }
    }
}
