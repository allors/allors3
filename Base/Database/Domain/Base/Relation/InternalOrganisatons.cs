// <copyright file="InternalOrganisatons.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Domain
{
    public partial class InternalOrganisations
    {
        protected override void BasePrepare(Setup setup)
        {
            setup.AddDependency(this.ObjectType, this.M.Locale.ObjectType);
            setup.AddDependency(this.ObjectType, this.M.TemplateType.ObjectType);
            setup.AddDependency(this.ObjectType, this.M.ShipmentMethod.ObjectType);
            setup.AddDependency(this.ObjectType, this.M.Carrier.ObjectType);
            setup.AddDependency(this.ObjectType, this.M.BillingProcess.ObjectType);
        }
    }
}
