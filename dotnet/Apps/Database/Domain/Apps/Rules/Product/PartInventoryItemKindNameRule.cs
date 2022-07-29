// <copyright file="PartInventoryItemKindNameRule.cs" company="Allors bvba">
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

    public class PartInventoryItemKindNameRule : Rule
    {
        public PartInventoryItemKindNameRule(MetaPopulation m) : base(m, new Guid("e2ef0855-63f6-4fe7-8cc9-88f22e318d01")) =>
            this.Patterns = new Pattern[]
            {
                m.Part.RolePattern(v => v.InventoryItemKind),
                m.InventoryItemKind.RolePattern(v => v.Name, v => v.PartsWhereInventoryItemKind),
            };

        public override void Derive(ICycle cycle, IEnumerable<IObject> matches)
        {
            var validation = cycle.Validation;

            foreach (var @this in matches.Cast<Part>())
            {
                @this.DerivePartInventoryItemKindName(validation);
            }
        }
    }

    public static class PartInventoryItemKindNameRuleExtensions
    {
        public static void DerivePartInventoryItemKindName(this Part @this, IValidation validation) => @this.InventoryItemKindName = @this.InventoryItemKind?.Name;
    }
}
