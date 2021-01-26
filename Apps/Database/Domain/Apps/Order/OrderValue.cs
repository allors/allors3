// <copyright file="OrderValue.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Database.Domain
{
    public partial class OrderValue
    {
        public void AppsOnPostDerive(ObjectOnPostDerive method) => method.Derivation.Validation.AssertAtLeastOne(this, this.M.OrderValue.FromAmount, this.M.OrderValue.ThroughAmount);
    }
}
