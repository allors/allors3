// <copyright file="TransitionalExtensions.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Database.Domain
{
    using System.Linq;

    public static partial class TransitionalExtensions
    {
        public static void CoreOnPostDerive(this Transitional @this, ObjectOnPostDerive method)
        {
            // Update PreviousObjectState and LastObjectState
            foreach (var transitionalConfiguration in @this.TransitionalConfigurations)
            {
                var objectState = @this.Strategy.GetCompositeRole(transitionalConfiguration.ObjectState);
                var lastObjectState = @this.Strategy.GetCompositeRole(transitionalConfiguration.LastObjectState);

                if (objectState?.Equals(lastObjectState) == false)
                {
                    @this.Strategy.SetCompositeRole(transitionalConfiguration.PreviousObjectState, lastObjectState);
                }

                @this.Strategy.SetCompositeRole(transitionalConfiguration.LastObjectState, objectState);
            }

            // Rollup ObjectStates, PreviousObjectState and LastObjectStates
            @this.RemoveObjectStates();
            @this.RemoveLastObjectStates();
            @this.RemovePreviousObjectStates();
            foreach (var transitionalConfiguration in @this.TransitionalConfigurations)
            {
                var objectState = (ObjectState)@this.Strategy.GetCompositeRole(transitionalConfiguration.ObjectState);
                var lastObjectState = (ObjectState)@this.Strategy.GetCompositeRole(transitionalConfiguration.LastObjectState);
                var previousObjectState = (ObjectState)@this.Strategy.GetCompositeRole(transitionalConfiguration.PreviousObjectState);
                @this.AddObjectState(objectState);
                @this.AddLastObjectState(lastObjectState);
                @this.AddPreviousObjectState(previousObjectState);
            }
        }

        public static bool HasChangedStates(this Transitional @this) =>
            @this.LastObjectStates.Count() != @this.ObjectStates.Count() || @this.LastObjectStates.Except(@this.ObjectStates).Any();
    }
}
