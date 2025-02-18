// <copyright file="OrderKind.cs" company="Allors bv">
// Copyright (c) Allors bv. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Database.Domain
{
    public partial class OrderKind
    {
        public void AppsOnBuild(ObjectOnBuild method)
        {
            if (!this.ExistScheduleManually)
            {
                this.ScheduleManually = false;
            }
        }
    }
}
