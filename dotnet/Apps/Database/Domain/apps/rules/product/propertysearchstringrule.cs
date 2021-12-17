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

    public class PropertySearchStringRule : Rule
    {
        public PropertySearchStringRule(MetaPopulation m) : base(m, new Guid("9a0f5042-dba5-4700-82de-ff0c7ae0ae17")) =>
            this.Patterns = new Pattern[]
        {
            m.FixedAsset.RolePattern(v => v.Name, m.Property),
            m.FixedAsset.RolePattern(v => v.LocalisedNames, m.Property),
            m.LocalisedText.RolePattern(v => v.Text, v => v.FixedAssetWhereLocalisedName.FixedAsset, m.Property),
            m.FixedAsset.RolePattern(v => v.LocalisedDescriptions, m.Property),
            m.LocalisedText.RolePattern(v => v.Text, v => v.FixedAssetWhereLocalisedDescription.FixedAsset, m.Property),
            m.FixedAsset.RolePattern(v => v.Keywords, m.Property),
            m.FixedAsset.RolePattern(v => v.LocalisedKeywords, m.Property),
            m.LocalisedText.RolePattern(v => v.Text, v => v.FixedAssetWhereLocalisedKeyword.FixedAsset, m.Property),

            m.FixedAsset.AssociationPattern(v => v.PartyFixedAssetAssignmentsWhereFixedAsset, m.Vehicle),
            m.Party.RolePattern(v => v.DisplayName, v => v.PartyFixedAssetAssignmentsWhereParty.PartyFixedAssetAssignment.FixedAsset.FixedAsset, m.Vehicle),
            m.AssetAssignmentStatus.RolePattern(v => v.Name, v => v.PartyFixedAssetAssignmentsWhereAssetAssignmentStatus.PartyFixedAssetAssignment.FixedAsset.FixedAsset, m.Vehicle),
            m.FixedAsset.AssociationPattern(v => v.WorkEffortFixedAssetAssignmentsWhereFixedAsset, m.Vehicle),
            m.FixedAsset.AssociationPattern(v => v.WorkRequirementsWhereFixedAsset, m.Vehicle),
        };

        public override void Derive(ICycle cycle, IEnumerable<IObject> matches)
        {
            foreach (var @this in matches.Cast<Property>())
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