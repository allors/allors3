// <copyright file="AccumulatedScoreDerivation.cs" company="Allors bvba">
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

    public class AccumulatedScoreRule : Rule
    {
        public AccumulatedScoreRule(MetaPopulation m) : base(m, new Guid("C280BFB1-543D-4B78-9397-DC956E032689")) =>
            this.Patterns = new Pattern[]
            {
                new RolePattern(m.Scoreboard.AccumulatedScores),
                new RolePattern(m.Score.Value) {Steps = new IPropertyType[]
                {
                    m.Score.GameWhereScore,
                    m.Game.ScoreboardWhereGame,
                }},
            };

        public override void Derive(IDomainDerivationCycle cycle, IEnumerable<IObject> matches)
        {
            foreach (var scoreboard in matches.Cast<Scoreboard>())
            {
                foreach (Score accumulatedScore in scoreboard.AccumulatedScores)
                {
                    accumulatedScore.Value =
                        scoreboard.Games.SelectMany(v => v.Scores)
                            .Where(v => v.Player == accumulatedScore.Player)
                            .Aggregate(0, (acc, v) => acc + v.Value ?? 0);
                }
            }
        }
    }
}
