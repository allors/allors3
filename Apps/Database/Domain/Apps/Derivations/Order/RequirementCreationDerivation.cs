// <copyright file="PartyFinancialRelationshipCreationDerivation.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Domain
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Meta;

    public class RequirementCreationDerivation : DomainDerivation
    {
        public RequirementCreationDerivation(M m) : base(m, new Guid("e51f66bc-5a19-442f-a478-466511b3c856")) =>
            this.Patterns = new Pattern[]
        {
            new CreatedPattern(m.Requirement.Class)
        };

        public override void Derive(IDomainDerivationCycle cycle, IEnumerable<IObject> matches)
        {
            foreach (var @this in matches.Cast<Requirement>())
            {
                if (!@this.ExistRequirementState)
                {
                    @this.RequirementState = new RequirementStates(@this.Strategy.Session).Active;
                }
            }
        }
    }
}
