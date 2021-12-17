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

    public class TeleCommunicationsNumberDisplayNameRule : Rule
    {
        public TeleCommunicationsNumberDisplayNameRule(MetaPopulation m) : base(m, new Guid("62d5090e-d88f-4ae4-bde2-d5ec513e4c43")) =>
            this.Patterns = new Pattern[]
            {
                m.TelecommunicationsNumber.RolePattern(v => v.CountryCode),
                m.TelecommunicationsNumber.RolePattern(v => v.AreaCode),
                m.TelecommunicationsNumber.RolePattern(v => v.ContactNumber),
            };

        public override void Derive(ICycle cycle, IEnumerable<IObject> matches)
        {
            foreach (var @this in matches.Cast<TelecommunicationsNumber>())
            {
                var array = new string[] { @this.CountryCode, @this.AreaCode, @this.ContactNumber };

                if (array.Any(s => !string.IsNullOrEmpty(s)))
                {
                    @this.DisplayName = string.Join(" ", array.Where(s => !string.IsNullOrEmpty(s)));
                }
            }
        }
    }
}
