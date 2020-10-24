// <copyright file="ChangedRoles.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>
// <summary>Defines the IDomainDerivation type.</summary>

namespace Allors.Domain.Derivations.Validating
{
    public static class ChangedRolePatternExtensions
    {
        public static void CreatePropertyMatches(this ChangedRolePattern @this, IStrategy rootMatch, Matcher matcher)
        {
            var matches = new[] { rootMatch };

            matches = @this.CreatePropertyMatches(rootMatch, matcher, matches);

            foreach (var match in matches)
            {
                matcher.Add(match, @this.RoleType);
            }
        }
    }
}
