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

    public class EquipmentSearchStringRule : Rule
    {
        public EquipmentSearchStringRule(MetaPopulation m) : base(m, new Guid("8bd46acb-0926-4ad7-bfeb-8a01638a1c98")) =>
            this.Patterns = new Pattern[]
        {
            m.FixedAsset.RolePattern(v => v.Name, m.Equipment),
            m.FixedAsset.RolePattern(v => v.LocalisedNames, m.Equipment),
            m.LocalisedText.RolePattern(v => v.Text, v => v.FixedAssetWhereLocalisedName.FixedAsset, m.Equipment),
            m.FixedAsset.RolePattern(v => v.LocalisedDescriptions, m.Equipment),
            m.LocalisedText.RolePattern(v => v.Text, v => v.FixedAssetWhereLocalisedDescription.FixedAsset, m.Equipment),
            m.FixedAsset.RolePattern(v => v.Keywords, m.Equipment),
            m.FixedAsset.RolePattern(v => v.LocalisedKeywords, m.Equipment),
            m.LocalisedText.RolePattern(v => v.Text, v => v.FixedAssetWhereLocalisedKeyword.FixedAsset, m.Equipment),

            m.FixedAsset.AssociationPattern(v => v.PartyFixedAssetAssignmentsWhereFixedAsset, m.Equipment),
            m.Party.RolePattern(v => v.DisplayName, v => v.PartyFixedAssetAssignmentsWhereParty.PartyFixedAssetAssignment.FixedAsset.FixedAsset, m.Equipment),
            m.AssetAssignmentStatus.RolePattern(v => v.Name, v => v.PartyFixedAssetAssignmentsWhereAssetAssignmentStatus.PartyFixedAssetAssignment.FixedAsset.FixedAsset, m.Equipment),
            m.FixedAsset.AssociationPattern(v => v.WorkEffortFixedAssetAssignmentsWhereFixedAsset, m.Equipment),
            m.FixedAsset.AssociationPattern(v => v.WorkRequirementsWhereFixedAsset, m.Equipment),
       };

        public override void Derive(ICycle cycle, IEnumerable<IObject> matches)
        {
            foreach (var @this in matches.Cast<Equipment>())
            {
                var array = new string[] {
                    @this.Name,
                    @this.ExistLocalisedNames ? string.Join(" ", @this.LocalisedNames?.Select(v => v.Text)) : null,
                    @this.Description,
                    @this.ExistLocalisedDescriptions ? string.Join(" ", @this.LocalisedDescriptions?.Select(v => v.Text)) : null,
                    @this.Keywords,
                    @this.ExistLocalisedKeywords ? string.Join(" ", @this.LocalisedKeywords?.Select(v => v.Text)) : null,
                    @this.ExistPartyFixedAssetAssignmentsWhereFixedAsset ? string.Join(" ", @this.PartyFixedAssetAssignmentsWhereFixedAsset?.Select(v => v.Party?.DisplayName)) : null,
                    @this.ExistPartyFixedAssetAssignmentsWhereFixedAsset ? string.Join(" ", @this.PartyFixedAssetAssignmentsWhereFixedAsset?.Select(v => v.AssetAssignmentStatus?.Name)) : null,
                    @this.ExistWorkEffortFixedAssetAssignmentsWhereFixedAsset ? string.Join(" ", @this.WorkEffortFixedAssetAssignmentsWhereFixedAsset?.Select(v => v.Assignment?.WorkEffortNumber)) : null,
                    @this.ExistWorkRequirementsWhereFixedAsset ? string.Join(" ", @this.WorkRequirementsWhereFixedAsset?.Select(v => v.RequirementNumber)) : null,
                };

                if (array.Any(s => !string.IsNullOrEmpty(s)))
                {
                    @this.SearchString = string.Join(" ", array.Where(s => !string.IsNullOrEmpty(s)));
                }
            }
        }
    }
}
