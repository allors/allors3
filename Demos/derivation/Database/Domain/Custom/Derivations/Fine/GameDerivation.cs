// <copyright file="GameDerivation.cs" company="Allors bvba">
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

    public class GameDerivation : DomainDerivation
    {
        public GameDerivation(M m) : base(m, new Guid("6C84C0DD-2855-403A-934B-D2990063A669")) =>
            this.Patterns = new Pattern[]
            {
                new ChangedPattern(m.Game.GameMode),
                new ChangedPattern(m.Game.StartDate),
                new ChangedPattern(m.Scoreboard.Games) { Steps = new IPropertyType[]{m.Scoreboard.Games} },
                new ChangedPattern(m.Scoreboard.Players) { Steps = new IPropertyType[]{m.Scoreboard.Games} },
            };

        public override void Derive(IDomainDerivationCycle cycle, IEnumerable<IObject> matches)
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
                    var score = new ScoreBuilder(cycle.Session)
                        .WithPlayer(player)
                        .Build();

                    game.AddScore(score);
                }
            }
        }
    }
}
