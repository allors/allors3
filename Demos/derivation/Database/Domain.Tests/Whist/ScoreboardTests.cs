// <copyright file="Domain.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Domain
{
    using Allors.Domain.Derivations.Default;
    using Allors.Meta;
    using System.Linq;
    using Xunit;

    public class ScoreboardTests : DomainTest
    {
        private Scoreboard scoreboard;
        private Person player1;
        private Person player2;
        private Person player3;
        private Person player4;

        private GameModes GameTypes;

        public void Setup(DerivationTypes data)
        {
            this.RegisterDerivations(data);

            var people = new People(this.Session);

            this.player1 = people.FindBy(M.Person.UserName, "player1");
            this.player2 = people.FindBy(M.Person.UserName, "player2");
            this.player3 = people.FindBy(M.Person.UserName, "player3");
            this.player4 = people.FindBy(M.Person.UserName, "player4");

            this.scoreboard = new ScoreboardBuilder(this.Session)
                .WithPlayer(player1)
                .WithPlayer(player2)
                .WithPlayer(player3)
                .WithPlayer(player4)
                .Build();

            this.GameTypes = new GameModes(this.Session);

            this.Session.Derive();
        }

        [Theory]
        [MemberData(nameof(TestedDerivationTypes))]
        public void TestZeroTestWithValues(object data)
        {
            this.Setup((DerivationTypes)data);

            //Arrange
            var game = new GameBuilder(this.Session).Build();
            scoreboard.AddGame(game);

            this.Session.Derive();

            var scores = game.Scores.ToArray();

            //Act
            scores[0].Value = -5;
            scores[1].Value = -5;
            scores[2].Value = 5;
            scores[3].Value = 5;

            //Assert
            Assert.True(scoreboard.ZeroTest());
        }

        [Theory]
        [MemberData(nameof(TestedDerivationTypes))]
        public void TestZeroTestWithoutValues(object data)
        {
            this.Setup((DerivationTypes)data);

            //Arrange
            var game = new GameBuilder(this.Session).Build();
            scoreboard.AddGame(game);

            this.Session.Derive();

            var scores = game.Scores.ToArray();

            //Act
            scores[0].Value = null;
            scores[1].Value = null;
            scores[2].Value = null;
            scores[3].Value = null;

            //Assert
            Assert.True(scoreboard.ZeroTest());
        }

        [Theory]
        [MemberData(nameof(TestedDerivationTypes))]
        public void TestAccumulatedScoresWithOneGame(object data)
        {
            this.Setup((DerivationTypes)data);

            //Arrange
            var game = new GameBuilder(this.Session).Build();
            scoreboard.AddGame(game);

            game.StartDate = this.Session.Now();
            game.EndDate = game.StartDate.Value.AddHours(1);

            //Act
            game.GameMode = this.GameTypes.Solo;

            game.AddDeclarer(player1);
            game.ExtraTricks = 0;

            game.AddWinner(this.player1);

            var derive = new Derivation(this.Session);
            var validation = derive.Derive();

            //Assert
            Assert.Equal(6, scoreboard.AccumulatedScores.First(v => v.Player == player1).Value);
            Assert.Equal(-2, scoreboard.AccumulatedScores.First(v => v.Player == player2).Value);
            Assert.Equal(-2, scoreboard.AccumulatedScores.First(v => v.Player == player3).Value);
            Assert.Equal(-2, scoreboard.AccumulatedScores.First(v => v.Player == player4).Value);
        }

        [Theory]
        [MemberData(nameof(TestedDerivationTypes))]
        public void TestAccumulatedScoresWithTwoGames(object data)
        {
            this.Setup((DerivationTypes)data);

            //Arrange
            var game = new GameBuilder(this.Session).Build();
            scoreboard.AddGame(game);

            game.StartDate = this.Session.Now();
            game.EndDate = game.StartDate.Value.AddHours(1);

            var game2 = new GameBuilder(this.Session).Build();
            scoreboard.AddGame(game2);

            game2.StartDate = this.Session.Now();
            game2.EndDate = game2.StartDate.Value.AddHours(1);

            //Act
            game.GameMode = this.GameTypes.Solo;

            game.AddDeclarer(player1);
            game.ExtraTricks = 0;

            game.AddWinner(this.player1);

            game2.GameMode = this.GameTypes.Solo;

            game2.AddDeclarer(player1);
            game2.ExtraTricks = 0;

            game2.AddWinner(this.player1);

            this.Session.Derive();

            //Assert
            Assert.Equal(12, scoreboard.AccumulatedScores.First(v => v.Player == player1).Value);
            Assert.Equal(-4, scoreboard.AccumulatedScores.First(v => v.Player == player2).Value);
            Assert.Equal(-4, scoreboard.AccumulatedScores.First(v => v.Player == player3).Value);
            Assert.Equal(-4, scoreboard.AccumulatedScores.First(v => v.Player == player4).Value);
        }

        [Theory]
        [MemberData(nameof(TestedDerivationTypes))]
        public void TestAccumulatedScoresWithNoGames(object data)
        {
            this.Setup((DerivationTypes)data);

            //Arrange

            //Act
            this.Session.Derive();

            //Assert
            Assert.Equal(0, scoreboard.AccumulatedScores.First(v => v.Player == player1).Value);
            Assert.Equal(0, scoreboard.AccumulatedScores.First(v => v.Player == player2).Value);
            Assert.Equal(0, scoreboard.AccumulatedScores.First(v => v.Player == player3).Value);
            Assert.Equal(0, scoreboard.AccumulatedScores.First(v => v.Player == player4).Value);
        }

        [Theory]
        [MemberData(nameof(TestedDerivationTypes))]
        public void TestAccumulatedScoresWithMultipleGameTypes(object data)
        {
            this.Setup((DerivationTypes)data);

            //Arrange
            var game = new GameBuilder(this.Session).Build();
            scoreboard.AddGame(game);

            game.StartDate = this.Session.Now();
            game.EndDate = game.StartDate.Value.AddHours(1);

            var game2 = new GameBuilder(this.Session).Build();
            scoreboard.AddGame(game2);

            game2.StartDate = this.Session.Now();
            game2.EndDate = game2.StartDate.Value.AddHours(1);

            var game3 = new GameBuilder(this.Session).Build();
            scoreboard.AddGame(game3);

            game3.StartDate = this.Session.Now();
            game3.EndDate = game3.StartDate.Value.AddHours(1);

            //Act
            game.GameMode = this.GameTypes.Solo;

            game.AddDeclarer(player1);
            game.ExtraTricks = 0;

            game.AddWinner(this.player1);

            game2.GameMode = this.GameTypes.Trull;

            game2.AddDeclarer(player1);
            game2.AddDeclarer(player2);
            game2.ExtraTricks = 2;

            game2.AddWinner(this.player1);
            game2.AddWinner(this.player2);

            game3.GameMode = this.GameTypes.Misery;

            game3.AddDeclarer(player1);

            game3.AddWinner(this.player1);

            this.Session.Derive();

            //Assert
            Assert.Equal(29, scoreboard.AccumulatedScores.First(v => v.Player == player1).Value);
            Assert.Equal(1, scoreboard.AccumulatedScores.First(v => v.Player == player2).Value);
            Assert.Equal(-15, scoreboard.AccumulatedScores.First(v => v.Player == player3).Value);
            Assert.Equal(-15, scoreboard.AccumulatedScores.First(v => v.Player == player4).Value);
        }
    }
}
