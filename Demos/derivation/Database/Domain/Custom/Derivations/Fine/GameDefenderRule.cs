// <copyright file="GameDefenderDerivation.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Database.Domain
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Meta;
    using Derivations.Rules;

    public class GameDefenderRule : Rule
    {
        public GameDefenderRule(MetaPopulation m) : base(m, new Guid("498A40C3-068E-4AB8-8E28-0B9F6F2D9D90")) =>
            this.Patterns = new Pattern[]
            {
                new RolePattern(m.Game.Declarers),
            };
     
        public override void Derive(IDomainDerivationCycle cycle, IEnumerable<IObject> matches)
        {
            foreach (var game in matches.Cast<Game>())
            {
                game.Defenders = game.ScoreboardWhereGame?.Players.Except(game.Declarers).ToArray();
            }
        }
    }
}
