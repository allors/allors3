// <copyright file="WorkEffortType.cs" company="Allors bv">
// Copyright (c) Allors bv. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Database.Domain
{
    public partial class WorkEffortType
    {
        public void AppsOnPostDerive(ObjectOnPostDerive method) => method.Derivation.Validation.AssertExists(this, this.M.WorkEffortType.Description);
    }
}
