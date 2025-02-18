// <copyright file="PartModelNameRule.cs" company="Allors bv">
// Copyright (c) Allors bv. All rights reserved.
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

    public class PartModelNameRule : Rule
    {
        public PartModelNameRule(MetaPopulation m) : base(m, new Guid("f288acdd-4777-4dfa-b0c4-cf8b05235a76")) =>
            this.Patterns = new Pattern[]
            {
                m.Part.RolePattern(v => v.Model),
                m.Model.RolePattern(v => v.Name, v => v.PartsWhereModel),
            };

        public override void Derive(ICycle cycle, IEnumerable<IObject> matches)
        {
            var validation = cycle.Validation;

            foreach (var @this in matches.Cast<Part>())
            {
                @this.DerivePartModelName(validation);
            }
        }
    }

    public static class PartModelNameRuleExtensions
    {
        public static void DerivePartModelName(this Part @this, IValidation validation) => @this.ModelName = @this.Model?.Name;
    }
}
