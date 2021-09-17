// <copyright file="Organisation.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>
// <summary>Defines the Person type.</summary>

namespace Allors.Database.Domain
{
    public partial class Organisation
    {
        public void CustomOnPostDerive(ObjectOnPostDerive _) => this.PostDeriveTrigger = true;

        public void CustomToggleCanWrite(OrganisationToggleCanWrite method)
        {
            if (this.ExistRestrictions)
            {
                this.RemoveRestrictions();
            }
            else
            {
                var toggleRestriction = new Restrictions(this.strategy.Transaction).ToggleRestriction;
                this.AddRestriction(toggleRestriction);
            }
        }

        public void CustomJustDoIt(OrganisationJustDoIt _) => this.JustDidIt = true;

        public override string ToString() => this.Name;
    }
}
