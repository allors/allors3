// <copyright file="Domain.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Database.Domain
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Allors.Database.Meta;
    using Database.Derivations;

    public class CoarseRule : Rule
    {
        public CoarseRule(M m) : base(m, new Guid("2D54CAE9-D3A0-4D66-BBF5-BF988B7983D6")) =>
            this.Patterns = new Pattern[]
            {
                new RolePattern(m.Scoreboard.Games) { Steps = new IPropertyType[]{m.Scoreboard.Games} },
                new RolePattern(m.Scoreboard.Players) { Steps = new IPropertyType[]{m.Scoreboard.Games} },
                new RolePattern(m.Scoreboard.Players),
                new RolePattern(m.Game.Declarers),
                new RolePattern(m.Game.StartDate),
                new RolePattern(m.Game.EndDate),
                new RolePattern(m.Game.GameMode),
                new RolePattern(m.Game.Winners),
                new RolePattern(m.Scoreboard.AccumulatedScores),
                new RolePattern(m.Score.Value) {Steps = new IPropertyType[]
                {
                    m.Score.GameWhereScore,
                    m.Game.ScoreboardWhereGame,
                }},
            };

        public override void Derive(IDomainDerivationCycle cycle, IEnumerable<IObject> matches)
        {
            // TODO: implement cross derive optimizations

            #region ScoreboardDerivation

            foreach (var scoreboard in matches.OfType<Scoreboard>())
            {
                var players = new HashSet<Person>(scoreboard.Players);

                foreach (var score in scoreboard.AccumulatedScores.Where(v => !players.Contains(v.Player)))
                {
                    players.Remove(score.Player);
                    score.Delete();
                }

                players.ExceptWith(scoreboard.AccumulatedScores.Select(v => v.Player));

                foreach (var player in players)
                {
                    var score = new ScoreBuilder(cycle.Transaction)
                        .WithPlayer(player)
                        .WithValue(0)
                        .Build();

                    scoreboard.AddAccumulatedScore(score);
                }
            }

            #endregion

            #region GameDerivation

            foreach (var game in matches.OfType<Game>())
            {
                var players = new HashSet<Person>(game.ScoreboardWhereGame.Players);

                foreach (var score in game.Scores.Where(v => !players.Contains(v.Player)))
                {
                    players.Remove(score.Player);
                    score.Delete();
                }

                players.ExceptWith(game.Scores.Select(v => v.Player));

                foreach (var player in players)
                {
                    var score = new ScoreBuilder(cycle.Transaction)
                        .WithPlayer(player)
                        .Build();

                    game.AddScore(score);
                }
            }

            #endregion

            #region StartEndDate

            foreach (var game in matches.OfType<Game>())
            {
                if (game.StartDate.HasValue && game.EndDate.HasValue)
                {
                    if (game.EndDate.Value <= game.StartDate.Value)
                    {
                        cycle.Validation.AddError("Start date must be before end date.");
                    }
                }
            }

            #endregion

            #region GameDefenderDerivation

            foreach (var game in matches.OfType<Game>())
            {
                game.Defenders = game.ScoreboardWhereGame?.Players.Except(game.Declarers).ToArray();
            }

            #endregion

            #region ScoreDerivation

            foreach (var game in matches.OfType<Game>())
            {
                foreach (Score score in game.Scores)
                {
                    if (game.ExistEndDate && game.ExistGameMode)
                    {

                        var gameType = game.GameMode;
                        var declarers = game.Declarers.ToList();
                        var winners = game.Winners.ToList();

                        var winning = winners.Contains(score.Player);
                        var declaring = declarers.Contains(score.Player);

                        if (gameType.IsMisery || gameType.IsOpenMisery)
                        {
                            switch (declarers.Count)
                            {
                                case 1:
                                    if (declaring)
                                    {
                                        score.Value = winning ? 15 : -15;
                                    }
                                    else
                                    {
                                        score.Value = !winners.Any() ? 5 : -5;
                                    }

                                    break;
                                case 2:
                                    if (declaring)
                                    {
                                        switch (winners.Count)
                                        {
                                            case 0:
                                                score.Value = -15;
                                                break;
                                            case 1:
                                                score.Value = winning ? 15 : -15;
                                                break;
                                            default:
                                                score.Value = 15;
                                                break;
                                        }
                                    }
                                    else
                                    {
                                        switch (winners.Count)
                                        {
                                            case 0:
                                                score.Value = 15;
                                                break;
                                            case 1:
                                                score.Value = 0;
                                                break;
                                            default:
                                                score.Value = -15;
                                                break;
                                        }
                                    }

                                    break;
                                case 4:
                                    if (declaring)
                                    {
                                        switch (winners.Count)
                                        {
                                            case 0:
                                                score.Value = 0;
                                                break;
                                            case 1:
                                                score.Value = winning ? 45 : -15;
                                                break;
                                            case 2:
                                                score.Value = winning ? 15 : -15;
                                                break;
                                            default:
                                                score.Value = winning ? 15 : -45;
                                                break;
                                        }
                                    }

                                    break;
                            }

                            if (gameType.IsOpenMisery)
                            {
                                score.Value *= 2;
                            }
                        }

                        if (gameType.IsGrandSlam || gameType.IsSmallSlam || gameType.IsAbondance)
                        {
                            if (declaring)
                            {
                                score.Value = winning ? 15 : -15;
                            }
                            else
                            {
                                score.Value = !winners.Any() ? 5 : -5;
                            }

                            if (gameType.IsSmallSlam)
                            {
                                score.Value *= 2;
                            }

                            if (gameType.IsGrandSlam)
                            {
                                score.Value *= 3;
                            }
                        }


                        if (gameType.IsSolo || gameType.IsProposalAndAcceptance || gameType.IsTrull)
                        {
                            var numberOfDefendersPerPerson = 3;
                            var extraTricksToBeDouble = 8;

                            if (gameType.IsProposalAndAcceptance || gameType.IsTrull)
                            {
                                numberOfDefendersPerPerson = 1;
                                extraTricksToBeDouble = 5;
                            }

                            var extraTricks = score.GameWhereScore.ExtraTricks ?? 0;

                            if (declaring)
                            {
                                var points = (numberOfDefendersPerPerson * 2) +
                                             (numberOfDefendersPerPerson * extraTricks);
                                var pointsWon = points;
                                if (extraTricks == extraTricksToBeDouble)
                                {
                                    pointsWon = points * 2;
                                }

                                score.Value = winning ? pointsWon : -points;
                            }
                            else
                            {
                                var points = 2 + extraTricks;
                                if (extraTricks == extraTricksToBeDouble)
                                {
                                    points *= 2;
                                }

                                score.Value = !winners.Any() ? points : -points;
                            }

                            if (gameType.IsTrull)
                            {
                                score.Value *= 2;
                            }
                        }
                    }
                    else
                    {
                        score.RemoveValue();
                    }
                }
            }


            #endregion

            #region AccumulatedScoreDerivation

            foreach (var scoreboard in matches.OfType<Scoreboard>())
            {
                foreach (Score accumulatedScore in scoreboard.AccumulatedScores)
                {
                    accumulatedScore.Value =
                        scoreboard.Games.SelectMany(v => v.Scores)
                            .Where(v => v.Player == accumulatedScore.Player)
                            .Aggregate(0, (acc, v) => acc + v.Value ?? 0);
                }
            }

            #endregion
        }
    }

}
