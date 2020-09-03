// <copyright file="Domain.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Domain
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Allors.Meta;

    public class ScoreboardDerivation : IDomainDerivation
    {
        public Guid Id => new Guid("7C68E187-381C-44E3-8074-889549F2A9F3");

        public IEnumerable<Pattern> Patterns { get; } = new Pattern[]
        {
            new ChangedRolePattern(M.Scoreboard.Players),
        };

        public void Derive(IDomainDerivationCycle cycle, IEnumerable<IObject> matches)
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
                    var score = new ScoreBuilder(cycle.Session)
                        .WithPlayer(player)
                        .WithValue(0)
                        .Build();

                    scoreboard.AddAccumulatedScore(score);
                }
            }
        }
    }

    public class GameDerivation : IDomainDerivation
    {
        public Guid Id => new Guid("7233B02C-51AD-4C58-9F4A-B9A10ADF3FAD");

        public IEnumerable<Pattern> Patterns { get; } = new Pattern[]
        {
            new CreatedPattern(M.Game.Class),
            new ChangedRolePattern(M.Scoreboard.Players) { Steps = new IPropertyType[]{M.Scoreboard.Games} },
        };

        public void Derive(IDomainDerivationCycle cycle, IEnumerable<IObject> matches)
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

    public class StartEndDateDerivation : IDomainDerivation
    {
        public Guid Id => new Guid("B29A3067-EE4D-496D-91A3-DC5F0FF98868");

        public IEnumerable<Pattern> Patterns { get; } = new Pattern[]
        {
            new ChangedRolePattern(M.Game.StartDate),
            new ChangedRolePattern(M.Game.EndDate),
        };

        public void Derive(IDomainDerivationCycle cycle, IEnumerable<IObject> matches)
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

    public class GameDefenderDerivation : IDomainDerivation
    {
        public Guid Id => new Guid("42947E7A-49AD-4F56-93FE-AC4EF95CE006");

        public IEnumerable<Pattern> Patterns { get; } = new Pattern[]
        {
            new ChangedRolePattern(M.Game.Declarers),
        };

        public void Derive(IDomainDerivationCycle cycle, IEnumerable<IObject> matches)
        {
            foreach (var game in matches.Cast<Game>())
            {
                game.Defenders = game.ScoreboardWhereGame?.Players.Except(game.Declarers).ToArray();
            }
        }
    }
    
    public class ScoreDerivation : IDomainDerivation
    {
        public Guid Id => new Guid("F2A88213-E81E-4931-AE9E-773D5AC870DB");

        public IEnumerable<Pattern> Patterns { get; } = new Pattern[]
        {
            new ChangedRolePattern(M.Game.GameMode),
            new ChangedRolePattern(M.Game.Winners),
        };

        public void Derive(IDomainDerivationCycle cycle, IEnumerable<IObject> matches)
        {
            foreach (var game in matches.Cast<Game>())
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
        }
    }

    public class AccumulatedScoreDerivation : IDomainDerivation
    {
        public Guid Id => new Guid("948FF889-0C8D-4AB6-9D4E-5BF90472D53B");

        public IEnumerable<Pattern> Patterns { get; } = new Pattern[]
        {
            new ChangedRolePattern(M.Scoreboard.AccumulatedScores),
            new ChangedRolePattern(M.Score.Value) {Steps = new IPropertyType[]
            {
                M.Score.GameWhereScore,
                M.Game.ScoreboardWhereGame,
            }},
        };

        public void Derive(IDomainDerivationCycle cycle, IEnumerable<IObject> matches)
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
