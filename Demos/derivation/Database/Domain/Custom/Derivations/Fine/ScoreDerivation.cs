// <copyright file="ScoreDerivation.cs" company="Allors bvba">
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

    public class ScoreDerivation : DomainDerivation
    {
        public ScoreDerivation(M m) : base(m, new Guid("AD572FC7-CC70-4D3B-9526-D39A23C23FE5")) =>
            this.Patterns = new Pattern[]
            {
                new ChangedPattern(m.Game.GameMode),
                new ChangedPattern(m.Game.Winners),
            };

        public override void Derive(IDomainDerivationCycle cycle, IEnumerable<IObject> matches)
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
}
