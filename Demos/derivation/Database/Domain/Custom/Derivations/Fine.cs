// <copyright file="Domain.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Domain
{
    using System.Collections.Generic;
    using System.Linq;
    using Allors.Meta;

    public static partial class FineDabaseExtensions
    {
        public class GameDerivation : IDomainDerivation
        {
            public void Derive(ISession session, IChangeSet changeSet, IDomainValidation validation)
            {
                changeSet.AssociationsByRoleType.TryGetValue(M.Scoreboard.Games, out var scoreboards);
                var scoreboardsWithChangedGames = scoreboards?.Select(session.Instantiate).OfType<Scoreboard>();

                changeSet.RolesByAssociationType.TryGetValue(M.Scoreboard.Games.AssociationType, out var games);
                var changedGames = games?.Select(session.Instantiate).OfType<Game>();

                if (scoreboardsWithChangedGames?.Any() == true)
                {
                    foreach (Scoreboard scoreboardWithChangedGames in scoreboardsWithChangedGames)
                    {
                        foreach (Game game in scoreboardWithChangedGames.Games.Where(v => changedGames.Contains(v)).ToList())
                        {
                            foreach (Score score in game.Scores)
                            {
                                if (scoreboardWithChangedGames.Players.Contains(score.Player))
                                {
                                    scoreboardWithChangedGames.Players.Remove(score.Player);
                                }
                                else
                                {
                                    score.Delete();
                                }
                            }

                            foreach (Person player in scoreboardWithChangedGames.Players)
                            {
                                var score = new ScoreBuilder(game.Strategy.Session)
                                .Build();

                                ((ScoreDerivedRoles)score).Player = player;

                                game.AddScore(score);
                            }
                        }
                    }
                }
            }
        }

        public class ScoreboardDerivation : IDomainDerivation
        {
            public void Derive(ISession session, IChangeSet changeSet, IDomainValidation validation)
            {
                changeSet.AssociationsByRoleType.TryGetValue(M.Scoreboard.Players, out var players);
                var scoreboardsWithChangedPlayers = players?.Select(session.Instantiate).OfType<Scoreboard>();

                if (scoreboardsWithChangedPlayers?.Any() == true)
                {
                    foreach (Scoreboard scoreboardWithChangedPlayers in scoreboardsWithChangedPlayers)
                    {
                        foreach (Score score in scoreboardWithChangedPlayers.AccumulatedScores)
                        {
                            var player = score.Player;
                            if (!scoreboardWithChangedPlayers.Players.Contains(player))
                            {
                                score.Delete();
                            }
                        }

                        foreach (Person player in scoreboardWithChangedPlayers.Players)
                        {
                            var scores = scoreboardWithChangedPlayers.AccumulatedScores.ToArray();
                            var score = scores.FirstOrDefault(v => v.Player == player);

                            if (score == null)
                            {
                                score = new ScoreBuilder(scoreboardWithChangedPlayers.Session()).Build();
                                ((ScoreDerivedRoles)score).Player = player;
                                score.Value = 0;

                                scoreboardWithChangedPlayers.AddAccumulatedScore(score);
                            }
                        }
                    }
                }
            }
        }

        public class DefenderDerivation : IDomainDerivation
        {
            public void Derive(ISession session, IChangeSet changeSet, IDomainValidation validation)
            {
                changeSet.AssociationsByRoleType.TryGetValue(M.Game.Declarers, out var declarers);
                var gameWithChangedDeclarers = declarers?.Select(session.Instantiate).OfType<Game>();

                if (gameWithChangedDeclarers?.Any() == true)
                {
                    foreach (Game game in gameWithChangedDeclarers)
                    {
                        ((GameDerivedRoles)game).Defenders = game.ScoreboardWhereGame?.Players.Except(game.Declarers).ToArray();
                    }
                }
            }
        }

        public class StartEndDateDerivation : IDomainDerivation
        {
            public void Derive(ISession session, IChangeSet changeSet, IDomainValidation validation)
            {
                changeSet.AssociationsByRoleType.TryGetValue(M.Game.EndDate, out var endDate);
                var gameWithChangedEndDate = endDate?.Select(session.Instantiate).OfType<Game>();

                if (gameWithChangedEndDate?.Any() == true)
                {
                    foreach (Game game in gameWithChangedEndDate)
                    {
                        if (game.EndDate.Value <= game.StartDate.Value)
                        {
                            validation.AddError("Start date must be before end date.");
                        }
                    }
                }
            }
        }

        public class GameScoreDerivation : IDomainDerivation
        {
            public void Derive(ISession session, IChangeSet changeSet, IDomainValidation validation)
            {
                changeSet.AssociationsByRoleType.TryGetValue(M.Game.GameType, out var changedWinners);
                var newGame = changedWinners?.Select(session.Instantiate).OfType<Game>();

                if (newGame?.Any() == true)
                {
                    foreach (Game game in newGame)
                    {
                        foreach (Score score in game.Scores)
                        {
                            if (game.ExistEndDate && game.ExistGameType)
                            {

                                var gameType = game.GameType;
                                var declarers = game.Declarers.ToList();
                                var winners = game.Winners.ToList();

                                var winning = winners.Contains(score.Player);
                                var declaring = declarers.Contains(score.Player);

                                if (gameType.IsMiserie || gameType.IsMiserieOpTafel)
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
                                                score.Value = winners.Count() == 0 ? 5 : -5;
                                            }
                                            break;
                                        case 2:
                                            if (declaring)
                                            {
                                                switch (winners.Count())
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
                                                switch (winners.Count())
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
                                                switch (winners.Count())
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
                                    if (gameType.IsMiserieOpTafel)
                                    {
                                        score.Value = score.Value * 2;
                                    }
                                }

                                if (gameType.IsSoloSlim || gameType.IsSolo || gameType.IsAbondance)
                                {
                                    if (declaring)
                                    {
                                        score.Value = winning ? 15 : -15;
                                    }
                                    else
                                    {
                                        score.Value = winners.Count() == 0 ? 5 : -5;
                                    }

                                    if (gameType.IsSolo)
                                    {
                                        score.Value = score.Value * 2;
                                    }

                                    if (gameType.IsSoloSlim)
                                    {
                                        score.Value = score.Value * 3;
                                    }
                                }


                                if (gameType.IsAlleenGaan || gameType.IsVragenEnMeegaan || gameType.IsTroel)
                                {
                                    int aantalDefendersPerPersoon = 3;
                                    var overslagenOmDubbelTeZijn = 8;

                                    if (gameType.IsVragenEnMeegaan || gameType.IsTroel)
                                    {
                                        aantalDefendersPerPersoon = 1;
                                        overslagenOmDubbelTeZijn = 5;
                                    }

                                    var overslagen = score.GameWhereScore.Overslagen ?? 0;

                                    if (declaring)
                                    {
                                        int punten = aantalDefendersPerPersoon * 2 + (aantalDefendersPerPersoon * overslagen);
                                        int puntenWinst = punten;
                                        if (overslagen == overslagenOmDubbelTeZijn)
                                        {
                                            puntenWinst = punten * 2;
                                        }
                                        score.Value = winning ? puntenWinst : -punten;
                                    }
                                    else
                                    {
                                        int punten = 2 + (overslagen);
                                        if (overslagen == overslagenOmDubbelTeZijn)
                                        {
                                            punten = punten * 2;
                                        }
                                        score.Value = winners.Count() == 0 ? punten : -punten;
                                    }
                                    if (gameType.IsTroel)
                                    {
                                        score.Value = score.Value * 2;
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
        }

        public class AccumulatedScoreDerivation : IDomainDerivation
        {
            public void Derive(ISession session, IChangeSet changeSet, IDomainValidation validation)
            {
                changeSet.AssociationsByRoleType.TryGetValue(M.Score.Value, out var changedScore);
                var newScores = changedScore?.Select(session.Instantiate).OfType<Score>();

                if (newScores?.Any() == true)
                {
                    foreach (Score score in newScores)
                    {
                        if (score.ExistGameWhereScore)
                        {
                            var game = score.GameWhereScore;

                            if (game.ExistScoreboardWhereGame)
                            {
                                var scoreboard = game.ScoreboardWhereGame;

                                var accumulatedScore = scoreboard.AccumulatedScores.FirstOrDefault(v => v.Player == score.Player);

                                if (accumulatedScore != null)
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
            }
        }

        public static void FineRegisterDerivations(this IDatabase @this)
        {
            @this.DomainDerivationById[new System.Guid("D82BBC64-DE39-4090-B383-1F8F45A1C406")] = new DefenderDerivation();
            @this.DomainDerivationById[new System.Guid("6227F451-ECD1-4414-87B7-E2846EDBE6A6")] = new GameDerivation();
            @this.DomainDerivationById[new System.Guid("98B2018A-B2C8-41BB-A90A-411FE8F9B053")] = new GameScoreDerivation();
            @this.DomainDerivationById[new System.Guid("4629E407-B553-4A07-AC88-911AA8BD8492")] = new AccumulatedScoreDerivation();
            @this.DomainDerivationById[new System.Guid("8F80A5B8-9F6F-4760-AC65-E51E75C98021")] = new ScoreboardDerivation();
            @this.DomainDerivationById[new System.Guid("2CC56F1C-ED28-466E-8F2F-49060024CEDD")] = new StartEndDateDerivation();
        }
    }
}
