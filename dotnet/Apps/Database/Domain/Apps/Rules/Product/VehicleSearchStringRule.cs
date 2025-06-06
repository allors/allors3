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

    public class VehicleSearchStringRule : Rule
    {
        public VehicleSearchStringRule(MetaPopulation m) : base(m, new Guid("fbe7ce1b-9e91-467f-8b1d-075a531ee18f")) =>
            this.Patterns = new Pattern[]
        {
            m.FixedAsset.RolePattern(v => v.DisplayName, m.Vehicle),
            m.FixedAsset.RolePattern(v => v.LocalisedDescriptions, m.Vehicle),
            m.LocalisedText.RolePattern(v => v.Text, v => v.FixedAssetWhereLocalisedDescription.ObjectType, m.Vehicle),
            m.FixedAsset.RolePattern(v => v.Keywords, m.Vehicle),
            m.FixedAsset.RolePattern(v => v.LocalisedKeywords, m.Vehicle),
            m.LocalisedText.RolePattern(v => v.Text, v => v.FixedAssetWhereLocalisedKeyword.ObjectType, m.Vehicle),

            m.FixedAsset.AssociationPattern(v => v.PartyFixedAssetAssignmentsWhereFixedAsset, m.Vehicle),
            m.Party.RolePattern(v => v.DisplayName, v => v.PartyFixedAssetAssignmentsWhereParty.ObjectType.FixedAsset.ObjectType, m.Vehicle),
            m.AssetAssignmentStatus.RolePattern(v => v.Name, v => v.PartyFixedAssetAssignmentsWhereAssetAssignmentStatus.ObjectType.FixedAsset.ObjectType, m.Vehicle),
            m.FixedAsset.AssociationPattern(v => v.WorkEffortFixedAssetAssignmentsWhereFixedAsset, m.Vehicle),
            m.FixedAsset.AssociationPattern(v => v.WorkRequirementsWhereFixedAsset, m.Vehicle),
        };

        public override void Derive(ICycle cycle, IEnumerable<IObject> matches)
        {
            var validation = cycle.Validation;

            foreach (var @this in matches.Cast<Vehicle>())
            {
                @this.DeriveVehicleSearchString(validation);
            }
        }
    }

    public static class VehicleSearchStringRuleExtensions
    {
        public static void DeriveVehicleSearchString(this Vehicle @this, IValidation validation)
        {
            var array = new string[] {
                    @this.DisplayName,
                    @this.Description,
                    @this.ExistLocalisedDescriptions ? string.Join(" ", @this.LocalisedDescriptions?.Select(v => v.Text ?? string.Empty).ToArray()) : null,
                    @this.Keywords,
                    @this.ExistLocalisedKeywords ? string.Join(" ", @this.LocalisedKeywords?.Select(v => v.Text ?? string.Empty).ToArray()) : null,
                    @this.ExistPartyFixedAssetAssignmentsWhereFixedAsset ? string.Join(" ", @this.PartyFixedAssetAssignmentsWhereFixedAsset?.Select(v => v.Party?.DisplayName ?? string.Empty).ToArray()) : null,
                    @this.ExistPartyFixedAssetAssignmentsWhereFixedAsset ? string.Join(" ", @this.PartyFixedAssetAssignmentsWhereFixedAsset?.Select(v => v.AssetAssignmentStatus?.Name ?? string.Empty).ToArray()) : null,
                    @this.ExistWorkEffortFixedAssetAssignmentsWhereFixedAsset ? string.Join(" ", @this.WorkEffortFixedAssetAssignmentsWhereFixedAsset?.Select(v => v.Assignment?.WorkEffortNumber ?? string.Empty).ToArray()) : null,
                    @this.ExistWorkRequirementsWhereFixedAsset ? string.Join(" ", @this.WorkRequirementsWhereFixedAsset?.Select(v => v.RequirementNumber ?? string.Empty).ToArray()) : null,
                };

            if (array.Any(s => !string.IsNullOrEmpty(s)))
            {
                @this.SearchString = string.Join(" ", array.Where(s => !string.IsNullOrEmpty(s)));
            }
            else
            {
                @this.RemoveSearchString();
            }
        }
    }
}
