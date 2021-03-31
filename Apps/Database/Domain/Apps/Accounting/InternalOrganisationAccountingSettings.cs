// <copyright file="InternalOrganisationAccountingSettings.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Database.Domain
{
    using System;

    public partial class InternalOrganisationAccountingSettings
    {
        public void BaseOnBuild(ObjectOnBuild method)
        {
            if (!this.ExistSubAccountCounter)
            {
                this.SubAccountCounter = new CounterBuilder(this.Transaction()).WithUniqueId(Guid.NewGuid()).WithValue(0).Build();
            }

            if (!this.ExistFiscalYearStartMonth)
            {
                this.FiscalYearStartMonth = 1;
            }

            if (!this.ExistFiscalYearStartDay)
            {
                this.FiscalYearStartDay = 1;
            }
        }
    }
}
