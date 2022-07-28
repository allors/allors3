// <copyright file="Domain.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Database.Domain
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Database.Derivations;
    using Meta;
    using Derivations.Rules;

    public class NonSerialisedInventoryItemSearchStringRule : Rule
    {
        public NonSerialisedInventoryItemSearchStringRule(MetaPopulation m) : base(m, new Guid("991bc056-d7bf-40fe-b1e8-a02c330b8ebd")) =>
            this.Patterns = new Pattern[]
            {
                m.InventoryItem.RolePattern(v => v.Part, m.NonSerialisedInventoryItem),
                m.Part.RolePattern(v => v.SearchString, v => v.InventoryItemsWherePart, m.NonSerialisedInventoryItem),
                m.InventoryItem.RolePattern(v => v.Facility, m.NonSerialisedInventoryItem),
                m.Facility.RolePattern(v => v.Name, v => v.InventoryItemsWhereFacility, m.NonSerialisedInventoryItem),
                m.InventoryItem.RolePattern(v => v.UnitOfMeasure, m.NonSerialisedInventoryItem),
                m.NonSerialisedInventoryItem.RolePattern(v => v.NonSerialisedInventoryItemState),
            };

        public override void Derive(ICycle cycle, IEnumerable<IObject> matches)
        {
            var validation = cycle.Validation;

            foreach (var @this in matches.Cast<NonSerialisedInventoryItem>())
            {
                @this.DeriveNonSerialisedInventoryItemSearchString(validation);
            }
        }
    }

    public static class NonSerialisedInventoryItemSearchStringRuleExtensions
    {
        public static void DeriveNonSerialisedInventoryItemSearchString(this NonSerialisedInventoryItem @this, IValidation validation)
        {
            var array = new string[] {
                    @this.Part?.SearchString,
                    @this.Facility?.Name,
                    @this.UnitOfMeasure?.Name,
                    @this.NonSerialisedInventoryItemState?.Name,
                };

            @this.SearchString = string.Join(" ", array.Where(s => !string.IsNullOrEmpty(s)));
        }
    }
}
