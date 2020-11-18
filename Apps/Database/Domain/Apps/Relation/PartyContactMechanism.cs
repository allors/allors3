// <copyright file="PartyContactMechanism.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
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
                this.FromDate = this.Session().Now();
            }

            if (!this.ExistUseAsDefault)
            {
                this.UseAsDefault = false;
            }
        }
    }
}
