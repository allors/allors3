// <copyright file="WorkEffortGrandTotalDerivation.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>
namespace Allors.Database.Domain
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Allors.Database.Meta;
    using Allors.Database.Derivations;

    public class WorkEffortGrandTotalDerivation : DomainDerivation
    {
        public WorkEffortGrandTotalDerivation(M m) : base(m, new Guid("f3c42add-acb8-49f4-9e1f-5c67378355f2")) =>
            this.Patterns = new[]
            {
                new AssociationPattern(m.WorkEffort.TotalLabourRevenue),
                new AssociationPattern(m.WorkEffort.TotalMaterialRevenue),
                new AssociationPattern(m.WorkEffort.TotalSubContractedRevenue),
            };

        public override void Derive(IDomainDerivationCycle cycle, IEnumerable<IObject> matches)
        {
            foreach (var @this in matches.Cast<WorkEffort>())
            {
                @this.GrandTotal = Math.Round(@this.TotalLabourRevenue + @this.TotalMaterialRevenue + @this.TotalSubContractedRevenue, 2);
            }
        }
    }
}
