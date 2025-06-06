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

    public class EmailAddressDisplayNameRule : Rule
    {
        public EmailAddressDisplayNameRule(MetaPopulation m) : base(m, new Guid("07fc1077-7939-4986-ba45-f63c62564e35")) =>
            this.Patterns = new Pattern[]
            {
                m.EmailAddress.RolePattern(v => v.ElectronicAddressString),
            };

        public override void Derive(ICycle cycle, IEnumerable<IObject> matches)
        {
            var validation = cycle.Validation;

            foreach (var @this in matches.Cast<EmailAddress>())
            {
                @this.DeriveEmailAddressDisplayName(validation);
            }
        }
    }

    public static class EmailAddressDisplayNameRuleExtensions
    {
        public static void DeriveEmailAddressDisplayName(this EmailAddress @this, IValidation validation)
        {
            @this.DisplayName = @this.ElectronicAddressString ?? "N/A";
        }
    }
}
