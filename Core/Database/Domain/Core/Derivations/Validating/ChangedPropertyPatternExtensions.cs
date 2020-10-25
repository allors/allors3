// <copyright file="ChangedRoles.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>
// <summary>Defines the IDomainDerivation type.</summary>

namespace Allors.Domain.Derivations.Validating
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Meta;

    public static class ChangedPropertyPatternExtensions
    {
        public static IEnumerable<IStrategy> CreatePropertyMatches(this ChangedPropertyPattern @this, IStrategy rootMatch, Matcher matcher)
        {
            IEnumerable<IStrategy> matches = new[] { rootMatch };

            if (@this.Steps?.Length > 0)
            {
                var propertyType = @this.Steps[@this.Steps.Length - 1];
                propertyType = propertyType is IRoleType asRoleType
                    ? (IPropertyType)asRoleType.AssociationType
                    : ((IAssociationType)propertyType).RoleType;
                matcher.Add(rootMatch, propertyType);

                for (var i = @this.Steps.Length - 1; i >= 0; i--)
                {
                    switch (@this.Steps[i])
                    {
                        case IRoleType roleType:

                            foreach (var match in matches)
                            {
                                matcher.Add(match, roleType.AssociationType);
                            }

                            matches = roleType.IsOne
                                ? matches.Select(v => v.GetCompositeAssociation(roleType.AssociationType).Strategy)
                                : matches.SelectMany(v => v.GetCompositeAssociations(roleType.AssociationType).Select(w => w.Strategy));

                            break;

                        case IAssociationType associationType:
                            foreach (var match in matches)
                            {
                                matcher.Add(match, associationType.RoleType);
                            }

                            matches = associationType.IsOne
                                ? matches.Select(v => v.GetCompositeRole(associationType.RoleType)?.Strategy).Where(v => v != null)
                                : matches.SelectMany(v => v.GetCompositeRoles(associationType.RoleType).Select(w => w.Strategy));

                            break;

                        default:
                            throw new NotSupportedException();
                    }
                }

            }

            return matches;
        }
    }
}
