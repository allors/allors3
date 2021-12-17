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

    public class VehicleSearchStringRule : Rule
    {
        public VehicleSearchStringRule(MetaPopulation m) : base(m, new Guid("fbe7ce1b-9e91-467f-8b1d-075a531ee18f")) =>
            this.Patterns = new Pattern[]
        {
            m.FixedAsset.RolePattern(v => v.Name, m.Vehicle),
            m.FixedAsset.RolePattern(v => v.LocalisedNames, m.Vehicle),
            m.LocalisedText.RolePattern(v => v.Text, v => v.FixedAssetWhereLocalisedName.FixedAsset, m.Vehicle),
            m.FixedAsset.RolePattern(v => v.LocalisedDescriptions, m.Vehicle),
            m.LocalisedText.RolePattern(v => v.Text, v => v.FixedAssetWhereLocalisedDescription.FixedAsset, m.Vehicle),
            m.FixedAsset.RolePattern(v => v.Keywords, m.Vehicle),
            m.FixedAsset.RolePattern(v => v.LocalisedKeywords, m.Vehicle),
            m.LocalisedText.RolePattern(v => v.Text, v => v.FixedAssetWhereLocalisedKeyword.FixedAsset, m.Vehicle),

            m.FixedAsset.AssociationPattern(v => v.PartyFixedAssetAssignmentsWhereFixedAsset, m.Vehicle),
            m.Party.RolePattern(v => v.DisplayName, v => v.PartyFixedAssetAssignmentsWhereParty.PartyFixedAssetAssignment.FixedAsset.FixedAsset, m.Vehicle),
            m.AssetAssignmentStatus.RolePattern(v => v.Name, v => v.PartyFixedAssetAssignmentsWhereAssetAssignmentStatus.PartyFixedAssetAssignment.FixedAsset.FixedAsset, m.Vehicle),
            m.FixedAsset.AssociationPattern(v => v.WorkEffortFixedAssetAssignmentsWhereFixedAsset, m.Vehicle),
            m.FixedAsset.AssociationPattern(v => v.WorkRequirementsWhereFixedAsset, m.Vehicle),
        };

        public override void Derive(ICycle cycle, IEnumerable<IObject> matches)
        {
            foreach (var @this in matches.Cast<Vehicle>())
            {
                var array = new string[] {
                    @this.Name,
                    string.Join(" ", @this.LocalisedNames?.Select(v => v.Text)),
                    @this.Description,
                    string.Join(" ", @this.LocalisedDescriptions?.Select(v => v.Text)),
                    @this.Keywords,
                    string.Join(" ", @this.LocalisedKeywords?.Select(v => v.Text)),
                    string.Join(" ", @this.PartyFixedAssetAssignmentsWhereFixedAsset?.Select(v => v.Party?.DisplayName)),
                    string.Join(" ", @this.PartyFixedAssetAssignmentsWhereFixedAsset?.Select(v => v.AssetAssignmentStatus?.Name)),
                    string.Join(" ", @this.WorkEffortFixedAssetAssignmentsWhereFixedAsset?.Select(v => v.Assignment?.WorkEffortNumber)),
                    string.Join(" ", @this.WorkRequirementsWhereFixedAsset?.Select(v => v.RequirementNumber)),
                };

                @this.SearchString = string.Join(" ", array.Where(s => !string.IsNullOrEmpty(s)));
            }
        }
    }
}
