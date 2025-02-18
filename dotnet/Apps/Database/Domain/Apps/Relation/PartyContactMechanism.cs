// <copyright file="PartyContactMechanism.cs" company="Allors bv">
// Copyright (c) Allors bv. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Database.Domain
{
    public partial class PartyContactMechanism
    {
        public void AppsOnBuild(ObjectOnBuild method)
        {
            if (!this.ExistFromDate)
            {
                this.FromDate = this.Transaction().Now();
            }

            if (!this.ExistUseAsDefault)
            {
                this.UseAsDefault = false;
            }
        }
    }
}
