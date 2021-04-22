// <copyright file="ExchangeRate.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Database.Domain
{
    public partial class ExchangeRate
    {
        public void BaseOnBuild(ObjectOnBuild method)
        {
            if (!this.ExistFactor)
            {
                this.Factor = 1;
            }
        }
    }
}
