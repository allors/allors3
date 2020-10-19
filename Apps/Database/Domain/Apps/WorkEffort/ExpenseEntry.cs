// <copyright file="ExpenseEntry.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Domain
{
    public partial class ExpenseEntry
    {
        public void AppsOnDerive(ObjectOnDerive method)
        {
            var derivation = method.Derivation;

            derivation.Validation.AssertAtLeastOne(this, this.M.ExpenseEntry.WorkEffort, this.M.ExpenseEntry.EngagementItem);
        }
    }
}