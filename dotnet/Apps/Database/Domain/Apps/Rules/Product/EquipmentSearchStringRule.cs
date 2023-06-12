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
            m.FixedAsset.RolePattern(v => v.DisplayName, m.Equipment),
            m.FixedAsset.RolePattern(v => v.LocalisedDescriptions, m.Equipment),
            m.LocalisedText.RolePattern(v => v.Text, v => v.FixedAssetWhereLocalisedDescription.ObjectType, m.Equipment),
            m.FixedAsset.RolePattern(v => v.Keywords, m.Equipment),
            m.FixedAsset.RolePattern(v => v.LocalisedKeywords, m.Equipment),
            m.LocalisedText.RolePattern(v => v.Text, v => v.FixedAssetWhereLocalisedKeyword.ObjectType, m.Equipment),

            m.FixedAsset.AssociationPattern(v => v.PartyFixedAssetAssignmentsWhereFixedAsset, m.Equipment),
            m.Party.RolePattern(v => v.DisplayName, v => v.PartyFixedAssetAssignmentsWhereParty.ObjectType.FixedAsset.ObjectType, m.Equipment),
            m.AssetAssignmentStatus.RolePattern(v => v.Name, v => v.PartyFixedAssetAssignmentsWhereAssetAssignmentStatus.ObjectType.FixedAsset.ObjectType, m.Equipment),
            m.FixedAsset.AssociationPattern(v => v.WorkEffortFixedAssetAssignmentsWhereFixedAsset, m.Equipment),
            m.FixedAsset.AssociationPattern(v => v.WorkRequirementsWhereFixedAsset, m.Equipment),
       };

        public override void Derive(ICycle cycle, IEnumerable<IObject> matches)
        {
            var validation = cycle.Validation;

            foreach (var @this in matches.Cast<Equipment>())
            {
                @this.DeriveEquipmentSearchString(validation);
            }
        }
    }

    public static class EquipmentSearchStringRuleExtensions
    {
        public static void DeriveEquipmentSearchString(this Equipment @this, IValidation validation)
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
