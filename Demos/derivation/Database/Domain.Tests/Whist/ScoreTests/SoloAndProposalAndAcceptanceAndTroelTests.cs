// <copyright file="Domain.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Database.Domain.Tests.Whist.Score
{
    using System.Linq;
    using Xunit;

    public class SoloAndProposalAndAcceptanceAndTroelTests : DomainTest, IClassFixture<Fixture>
    {
        public SoloAndProposalAndAcceptanceAndTroelTests(Fixture fixture) : base(fixture) { }

        private Scoreboard scoreboard;
        private Person player1;
        private Person player2;
        private Person player3;
        private Person player4;

        private GameModes GameModes;

        public void Setup(DerivationTypes data)
        {
            this.RegisterAdditionalDerivations(data);

            var people = new People(this.Transaction);

            this.player1 = people.FindBy(M.Person.UserName, "player1");
            this.player2 = people.FindBy(M.Person.UserName, "player2");
            this.player3 = people.FindBy(M.Person.UserName, "player3");
            this.player4 = people.FindBy(M.Person.UserName, "player4");

            this.scoreboard = new ScoreboardBuilder(this.Transaction)
                .WithPlayer(player1)
                .WithPlayer(player2)
                .WithPlayer(player3)
                .WithPlayer(player4)
                .Build();

            this.GameModes = new GameModes(this.Transaction);

            this.Transaction.Derive();
        }

        [Theory]
        [MemberData(nameof(TestedDerivationTypes))]
        public void TestSoloWithOneDeclarerAndNoWinnerAndNoTricks(object data)
        {
            this.Setup((DerivationTypes)data);

            //Arrange
            var game = new GameBuilder(this.Transaction).Build();
            scoreboard.AddGame(game);

            game.StartDate = this.Transaction.Now();
            game.EndDate = game.StartDate.Value.AddHours(1);

            //Act
            game.GameMode = this.GameModes.Solo;
            game.AddDeclarer(player1);

            this.Transaction.Derive();

            //Assert
            Assert.Equal(-6, game.Scores.First(v => v.Player == player1).Value);
            Assert.Equal(2, game.Scores.First(v => v.Player == player2).Value);
            Assert.Equal(2, game.Scores.First(v => v.Player == player3).Value);
            Assert.Equal(2, game.Scores.First(v => v.Player == player4).Value);
            Assert.True(this.scoreboard.ZeroTest());
        }

        [Theory]
        [MemberData(nameof(TestedDerivationTypes))]
        public void TestSoloWithOneDeclarerAndNoWinnerAndTricks(object data)
        {
            this.Setup((DerivationTypes)data);

            //Arrange
            var game = new GameBuilder(this.Transaction).Build();
            scoreboard.AddGame(game);

            game.StartDate = this.Transaction.Now();
            game.EndDate = game.StartDate.Value.AddHours(1);

            //Act
            game.GameMode = this.GameModes.Solo;
            game.AddDeclarer(player1);
            game.ExtraTricks = 3;

            this.Transaction.Derive();

            //Assert
            Assert.Equal(-15, game.Scores.First(v => v.Player == player1).Value);
            Assert.Equal(5, game.Scores.First(v => v.Player == player2).Value);
            Assert.Equal(5, game.Scores.First(v => v.Player == player3).Value);
            Assert.Equal(5, game.Scores.First(v => v.Player == player4).Value);
            Assert.True(this.scoreboard.ZeroTest());
        }

        [Theory]
        [MemberData(nameof(TestedDerivationTypes))]
        public void TestSoloWithOneDeclarerAndOneWinnerAndNoTricks(object data)
        {
            this.Setup((DerivationTypes)data);

            //Arrange
            var game = new GameBuilder(this.Transaction).Build();
            scoreboard.AddGame(game);

            game.StartDate = this.Transaction.Now();
            game.EndDate = game.StartDate.Value.AddHours(1);

            //Act
            game.GameMode = this.GameModes.Solo;
            game.AddDeclarer(player1);
            game.AddWinner(player1);

            this.Transaction.Derive();

            //Assert
            Assert.Equal(6, game.Scores.First(v => v.Player == player1).Value);
            Assert.Equal(-2, game.Scores.First(v => v.Player == player2).Value);
            Assert.Equal(-2, game.Scores.First(v => v.Player == player3).Value);
            Assert.Equal(-2, game.Scores.First(v => v.Player == player4).Value);
            Assert.True(this.scoreboard.ZeroTest());
        }

        [Theory]
        [MemberData(nameof(TestedDerivationTypes))]
        public void TestSoloWithOneDeclarerAndOneWinnerAndTricks(object data)
        {
            this.Setup((DerivationTypes)data);

            //Arrange
            var game = new GameBuilder(this.Transaction).Build();
            scoreboard.AddGame(game);

            game.StartDate = this.Transaction.Now();
            game.EndDate = game.StartDate.Value.AddHours(1);

            //Act
            game.GameMode = this.GameModes.Solo;
            game.AddDeclarer(player1);
            game.AddWinner(player1);
            game.ExtraTricks = 3;

            this.Transaction.Derive();

            //Assert
            Assert.Equal(15, game.Scores.First(v => v.Player == player1).Value);
            Assert.Equal(-5, game.Scores.First(v => v.Player == player2).Value);
            Assert.Equal(-5, game.Scores.First(v => v.Player == player3).Value);
            Assert.Equal(-5, game.Scores.First(v => v.Player == player4).Value);
            Assert.True(this.scoreboard.ZeroTest());
        }

        [Theory]
        [MemberData(nameof(TestedDerivationTypes))]
        public void TestSoloWithOneDeclarerAndOneWinnerAndAllTricks(object data)
        {
            this.Setup((DerivationTypes)data);

            //Arrange
            var game = new GameBuilder(this.Transaction).Build();
            scoreboard.AddGame(game);

            game.StartDate = this.Transaction.Now();
            game.EndDate = game.StartDate.Value.AddHours(1);

            //Act
            game.GameMode = this.GameModes.Solo;
            game.AddDeclarer(player1);
            game.AddWinner(player1);
            game.ExtraTricks = 8;

            this.Transaction.Derive();

            //Assert
            Assert.Equal(60, game.Scores.First(v => v.Player == player1).Value);
            Assert.Equal(-20, game.Scores.First(v => v.Player == player2).Value);
            Assert.Equal(-20, game.Scores.First(v => v.Player == player3).Value);
            Assert.Equal(-20, game.Scores.First(v => v.Player == player4).Value);
            Assert.True(this.scoreboard.ZeroTest());
        }

        [Theory]
        [MemberData(nameof(TestedDerivationTypes))]
        public void TestProposalAndAcceptanceWithTwoDeclarersAndNoWinnersAndNoTricks(object data)
        {
            this.Setup((DerivationTypes)data);

            //Arrange
            var game = new GameBuilder(this.Transaction).Build();
            scoreboard.AddGame(game);

            game.StartDate = this.Transaction.Now();
            game.EndDate = game.StartDate.Value.AddHours(1);

            //Act
            game.GameMode = this.GameModes.ProposalAndAcceptance;
            game.AddDeclarer(player1);
            game.AddDeclarer(player2);

            this.Transaction.Derive();

            //Assert
            Assert.Equal(-2, game.Scores.First(v => v.Player == player1).Value);
            Assert.Equal(-2, game.Scores.First(v => v.Player == player2).Value);
            Assert.Equal(2, game.Scores.First(v => v.Player == player3).Value);
            Assert.Equal(2, game.Scores.First(v => v.Player == player4).Value);
            Assert.True(this.scoreboard.ZeroTest());
        }

        [Theory]
        [MemberData(nameof(TestedDerivationTypes))]
        public void TestProposalAndAcceptanceWithTwoDeclarersAndNoWinnersAndTricks(object data)
        {
            this.Setup((DerivationTypes)data);

            //Arrange
            var game = new GameBuilder(this.Transaction).Build();
            scoreboard.AddGame(game);

            game.StartDate = this.Transaction.Now();
            game.EndDate = game.StartDate.Value.AddHours(1);

            //Act
            game.GameMode = this.GameModes.ProposalAndAcceptance;
            game.AddDeclarer(player1);
            game.AddDeclarer(player2);

            game.ExtraTricks = 3;

            this.Transaction.Derive();

            //Assert
            Assert.Equal(-5, game.Scores.First(v => v.Player == player1).Value);
            Assert.Equal(-5, game.Scores.First(v => v.Player == player2).Value);
            Assert.Equal(5, game.Scores.First(v => v.Player == player3).Value);
            Assert.Equal(5, game.Scores.First(v => v.Player == player4).Value);
            Assert.True(this.scoreboard.ZeroTest());
        }

        [Theory]
        [MemberData(nameof(TestedDerivationTypes))]
        public void TestProposalAndAcceptanceWithTwoDeclarersAndTwoWinnersAndNoTricks(object data)
        {
            this.Setup((DerivationTypes)data);

            //Arrange
            var game = new GameBuilder(this.Transaction).Build();
            scoreboard.AddGame(game);

            game.StartDate = this.Transaction.Now();
            game.EndDate = game.StartDate.Value.AddHours(1);

            //Act
            game.GameMode = this.GameModes.ProposalAndAcceptance;
            game.AddDeclarer(player1);
            game.AddDeclarer(player2);

            game.AddWinner(player1);
            game.AddWinner(player2);

            this.Transaction.Derive();

            //Assert
            Assert.Equal(2, game.Scores.First(v => v.Player == player1).Value);
            Assert.Equal(2, game.Scores.First(v => v.Player == player2).Value);
            Assert.Equal(-2, game.Scores.First(v => v.Player == player3).Value);
            Assert.Equal(-2, game.Scores.First(v => v.Player == player4).Value);
            Assert.True(this.scoreboard.ZeroTest());
        }

        [Theory]
        [MemberData(nameof(TestedDerivationTypes))]
        public void TestProposalAndAcceptanceWithTwoDeclarersAndTwoWinnersAndTricks(object data)
        {
            this.Setup((DerivationTypes)data);

            //Arrange
            var game = new GameBuilder(this.Transaction).Build();
            scoreboard.AddGame(game);

            game.StartDate = this.Transaction.Now();
            game.EndDate = game.StartDate.Value.AddHours(1);

            //Act
            game.GameMode = this.GameModes.ProposalAndAcceptance;
            game.AddDeclarer(player1);
            game.AddDeclarer(player2);

            game.AddWinner(player1);
            game.AddWinner(player2);

            game.ExtraTricks = 3;

            this.Transaction.Derive();

            //Assert
            Assert.Equal(5, game.Scores.First(v => v.Player == player1).Value);
            Assert.Equal(5, game.Scores.First(v => v.Player == player2).Value);
            Assert.Equal(-5, game.Scores.First(v => v.Player == player3).Value);
            Assert.Equal(-5, game.Scores.First(v => v.Player == player4).Value);
            Assert.True(this.scoreboard.ZeroTest());
        }

        [Theory]
        [MemberData(nameof(TestedDerivationTypes))]
        public void TestProposalAndAcceptanceWithTwoDeclarersAndTwoWinnersAndAllTricks(object data)
        {
            this.Setup((DerivationTypes)data);

            //Arrange
            var game = new GameBuilder(this.Transaction).Build();
            scoreboard.AddGame(game);

            game.StartDate = this.Transaction.Now();
            game.EndDate = game.StartDate.Value.AddHours(1);

            //Act
            game.GameMode = this.GameModes.ProposalAndAcceptance;
            game.AddDeclarer(player1);
            game.AddDeclarer(player2);

            game.AddWinner(player1);
            game.AddWinner(player2);

            game.ExtraTricks = 5;

            this.Transaction.Derive();

            //Assert
            Assert.Equal(14, game.Scores.First(v => v.Player == player1).Value);
            Assert.Equal(14, game.Scores.First(v => v.Player == player2).Value);
            Assert.Equal(-14, game.Scores.First(v => v.Player == player3).Value);
            Assert.Equal(-14, game.Scores.First(v => v.Player == player4).Value);
            Assert.True(this.scoreboard.ZeroTest());
        }

        [Theory]
        [MemberData(nameof(TestedDerivationTypes))]
        public void TestTrullWithTwoDeclarersAndTwoWinnersAndAllTricks(object data)
        {
            this.Setup((DerivationTypes)data);

            //Arrange
            var game = new GameBuilder(this.Transaction).Build();
            scoreboard.AddGame(game);

            game.StartDate = this.Transaction.Now();
            game.EndDate = game.StartDate.Value.AddHours(1);

            //Act
            game.GameMode = this.GameModes.Trull;
            game.AddDeclarer(player1);
            game.AddDeclarer(player2);

            game.AddWinner(player1);
            game.AddWinner(player2);

            game.ExtraTricks = 5;

            this.Transaction.Derive();

            //Assert
            Assert.Equal(28, game.Scores.First(v => v.Player == player1).Value);
            Assert.Equal(28, game.Scores.First(v => v.Player == player2).Value);
            Assert.Equal(-28, game.Scores.First(v => v.Player == player3).Value);
            Assert.Equal(-28, game.Scores.First(v => v.Player == player4).Value);
            Assert.True(this.scoreboard.ZeroTest());
        }

        [Theory]
        [MemberData(nameof(TestedDerivationTypes))]
        public void TestTrullWithTwoDeclarersAndTwoWinnersAndTricks(object data)
        {
            this.Setup((DerivationTypes)data);

            //Arrange
            var game = new GameBuilder(this.Transaction).Build();
            scoreboard.AddGame(game);

            game.StartDate = this.Transaction.Now();
            game.EndDate = game.StartDate.Value.AddHours(1);

            //Act
            game.GameMode = this.GameModes.Trull;
            game.AddDeclarer(player1);
            game.AddDeclarer(player2);

            game.AddWinner(player1);
            game.AddWinner(player2);

            game.ExtraTricks = 3;

            this.Transaction.Derive();

            //Assert
            Assert.Equal(10, game.Scores.First(v => v.Player == player1).Value);
            Assert.Equal(10, game.Scores.First(v => v.Player == player2).Value);
            Assert.Equal(-10, game.Scores.First(v => v.Player == player3).Value);
            Assert.Equal(-10, game.Scores.First(v => v.Player == player4).Value);
            Assert.True(this.scoreboard.ZeroTest());
        }
    }
}
