// <copyright file="Domain.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Domain
{
    using System.Collections.Generic;
    using System.Linq;
    using Allors.Domain.Derivations;
    using Allors.Meta;

    public static partial class CoarseDabaseExtensions
    {
        public class CompleteDerivation : IDomainDerivation
        {
            public void Derive(ISession session, IChangeSet changeSet, IDomainValidation validation)
            {
                var derivedScoreboards = new HashSet<Scoreboard>();

                #region Scoreboard

                changeSet.AssociationsByRoleType.TryGetValue(M.Scoreboard.Players, out var players);
                var scoreboardsWithChangedPlayers = players?.Select(session.Instantiate).OfType<Scoreboard>();

                if (scoreboardsWithChangedPlayers?.Any() == true)
                {
                    foreach (Scoreboard scoreboardWithChangedPlayers in scoreboardsWithChangedPlayers)
                    {
                        derivedScoreboards.Add(scoreboardWithChangedPlayers);

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
                                score.Player = player;
                                score.Value = 0;

                                scoreboardWithChangedPlayers.AddAccumulatedScore(score);
                            }
                        }
                    }
                }

                #endregion

                #region Game
                changeSet.AssociationsByRoleType.TryGetValue(M.Scoreboard.Games, out var scoreboards);
                var scoreboardsWithChangedGames = scoreboards?.Select(session.Instantiate).OfType<Scoreboard>();

                changeSet.RolesByAssociationType.TryGetValue(M.Scoreboard.Games.AssociationType, out var games);
                var changedGames = games?.Select(session.Instantiate).OfType<Game>();

                var changedScoreboards = new HashSet<Scoreboard>(derivedScoreboards);
                if(scoreboardsWithChangedGames != null)
                {
                    changedScoreboards.UnionWith(scoreboardsWithChangedGames);
                }

                if (changedScoreboards.Any())
                {
                    foreach (Scoreboard scoreboardWithChangedGames in changedScoreboards)
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

                                score.Player = player;

                                game.AddScore(score);
                            }
                        }
                    }
                }

                #endregion

                #region StartEndDate

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

                #endregion

                #region Defender

                changeSet.AssociationsByRoleType.TryGetValue(M.Game.Declarers, out var declarers);
                var gameWithChangedDeclarers = declarers?.Select(session.Instantiate).OfType<Game>();

                if (gameWithChangedDeclarers?.Any() == true)
                {
                    foreach (Game game in gameWithChangedDeclarers)
                    {
                        game.Defenders = game.ScoreboardWhereGame?.Players.Except(game.Declarers).ToArray();
                    }
                }

                #endregion

                #region GameScore

                var changedScores = new HashSet<Score>();

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
                                var gameDeclarers = game.Declarers.ToList();
                                var winners = game.Winners.ToList();

                                var winning = winners.Contains(score.Player);
                                var declaring = gameDeclarers.Contains(score.Player);

                                if (gameType.IsMiserie || gameType.IsMiserieOpTafel)
                                {
                                    switch (gameDeclarers.Count)
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

                                changedScores.Add(score);
                            }
                            else
                            {
                                score.RemoveValue();
                            }
                        }
                    }
                }

                #endregion

                #region AccumulatedScore

                changeSet.AssociationsByRoleType.TryGetValue(M.Score.Value, out var changedScore);
                var newScores = changedScore?.Select(session.Instantiate).OfType<Score>();

                var allChangedScores = new HashSet<Score>(changedScores);
                if(newScores != null)
                {
                    allChangedScores.UnionWith(newScores);
                }

                if (allChangedScores.Any())
                {
                    foreach (Score score in allChangedScores)
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

                #endregion
            }
        }

        public static void CoarseRegisterDerivations(this IDatabase @this)
        {
            @this.DomainDerivationById[new System.Guid("D684885C-8115-4BB0-99D6-6CB47A27FAE3")] = new CompleteDerivation();
                                                       
        }
    }
}
