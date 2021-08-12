// <copyright file="StartEndDateDerivation.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

using Allors.Database.Derivations;

namespace Allors.Database.Domain
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Meta;
    using Derivations.Rules;

    public class StartEndDateRule : Rule
    {
        public StartEndDateRule(MetaPopulation m) : base(m, new Guid("F9080FC9-04D8-4072-92E9-936D2C34D028")) =>
            this.Patterns = new Pattern[]
            {
                new RolePattern(m.Game.StartDate),
                new RolePattern(m.Game.EndDate),
            };
      
        public override void Derive(ICycle cycle, IEnumerable<IObject> matches)
        {
            foreach (var game in matches.Cast<Game>())
            {
                if (game.StartDate.HasValue && game.EndDate.HasValue && game.EndDate.Value <= game.StartDate.Value)
                {
                    cycle.Validation.AddError("Start date must be before end date.");
                }
            }
        }
    }
}
