// <copyright file="Domain.cs" company="Allors bv">
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

    public class WebAddressDisplayNameRule : Rule
    {
        public WebAddressDisplayNameRule(MetaPopulation m) : base(m, new Guid("899fd9da-9708-4f89-8438-3086b07b6c5e")) =>
            this.Patterns = new Pattern[]
            {
                m.WebAddress.RolePattern(v => v.ElectronicAddressString),
            };

        public override void Derive(ICycle cycle, IEnumerable<IObject> matches)
        {
            var validation = cycle.Validation;

            foreach (var @this in matches.Cast<WebAddress>())
            {
                @this.DeriveWebAddressDisplayNameRule(validation);
            }
        }
    }

    public static class WebAddressDisplayNameRuleExtensions
    {
        public static void DeriveWebAddressDisplayNameRule(this WebAddress @this, IValidation validation)
        {
            @this.DisplayName = @this.ElectronicAddressString ?? "N/A";
        }
    }
}
