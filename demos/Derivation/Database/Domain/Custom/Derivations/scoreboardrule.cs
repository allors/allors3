// <copyright file="ScoreboardDerivation.cs" company="Allors bvba">
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

    public class ScoreboardRule : Rule
    {
        public ScoreboardRule(MetaPopulation m) : base(m, new Guid("66659E72-C496-485C-A68D-E940A9A94F9C")) =>
            this.Patterns = new Pattern[]
            {
                new RolePattern(m.Scoreboard.Players),
                new RolePattern(m.Scoreboard.Games),
                m.Game.RolePattern(v=>v.Scores, v=>v.ScoreboardWhereGame)
            };

        public override void Derive(ICycle cycle, IEnumerable<IObject> matches)
        {
            foreach (var scoreboard in matches.Cast<Scoreboard>())
            {
                var players = new HashSet<Person>(scoreboard.Players);

                foreach (var score in scoreboard.AccumulatedScores.Where(v => !players.Contains(v.Player)))
                {
                    score.Delete();
                    players.Remove(score.Player);
                }

                foreach (var player in players)
                {
                    var score = new ScoreBuilder(cycle.Transaction)
                        .WithPlayer(player)
                        .WithValue(0)
                        .Build();

                    scoreboard.AddAccumulatedScore(score);
                }
            }
        }
    }
}
