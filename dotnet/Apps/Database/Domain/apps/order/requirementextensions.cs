// <copyright file="RequirementExtensions.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>


namespace Allors.Database.Domain
{
    public static partial class RequirementExtensions
    {
        public static void AppsOnBuild(this Requirement @this, ObjectOnBuild method)
        {
            if (!@this.ExistRequirementState)
            {
                @this.RequirementState = new RequirementStates(@this.Strategy.Transaction).Created;
            }
        }

        public static void AppsApprove(this Requirement @this, RequirementCancel method)
        {
            @this.RequirementState = new RequirementStates(@this.Strategy.Transaction).Cancelled;
            method.StopPropagation = true;
        }
    }
}
