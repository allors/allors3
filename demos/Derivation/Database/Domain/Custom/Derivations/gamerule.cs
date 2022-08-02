// <copyright file="GameDerivation.cs" company="Allors bvba">
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

    public class GameRule : Rule
    {
        public GameRule(MetaPopulation m) : base(m, new Guid("6C84C0DD-2855-403A-934B-D2990063A669")) =>
            this.Patterns = new Pattern[]
            {
                new RolePattern(m.Game.GameMode),
                new RolePattern(m.Game.StartDate),
                m.Scoreboard.RolePattern(v=>v.Games, v => v.Games),
                m.Scoreboard.RolePattern(v=>v.Players, v=>v.Games)
            };

        public override void Derive(ICycle cycle, IEnumerable<IObject> matches)
        {
            foreach (var game in matches.Cast<Game>())
            {
                var players = new HashSet<Person>(game.ScoreboardWhereGame.Players);

                foreach (var score in game.Scores.Where(v => !players.Contains(v.Player)))
                {
                    score.Delete();
                    players.Remove(score.Player);
                }

                foreach (var player in players)
                {
                    var score = new ScoreBuilder(cycle.Transaction)
                        .WithPlayer(player)
                        .Build();

                    game.AddScore(score);
                }
            }
        }
    }
}
