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

    public class PostalAddressDisplayNameRule : Rule
    {
        public PostalAddressDisplayNameRule(MetaPopulation m) : base(m, new Guid("5922677a-6e7d-4ada-bd7c-0104edb278d8")) =>
            this.Patterns = new Pattern[]
            {
                m.PostalAddress.RolePattern(v => v.Address1),
                m.PostalAddress.RolePattern(v => v.Address2),
                m.PostalAddress.RolePattern(v => v.Address3),
                m.PostalAddress.RolePattern(v => v.PostalCode),
                m.PostalAddress.RolePattern(v => v.Locality),
                m.PostalAddress.RolePattern(v => v.Country),
            };

        public override void Derive(ICycle cycle, IEnumerable<IObject> matches)
        {
            foreach (var @this in matches.Cast<PostalAddress>())
            {
                var array = new string[] {@this.Address1, @this.Address2, @this.Address3, @this.PostalCode, @this.Locality, @this.Country?.Name };
                @this.DisplayName = string.Join(", ", array.Where(s => !string.IsNullOrEmpty(s)));
            }
        }
    }
}
