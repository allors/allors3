//------------------------------------------------------------------------------------------------- 
// <copyright file="DemoTests.cs" company="Allors bvba">
// Copyright 2002-2009 Allors bvba.
// 
// Dual Licensed under
//   a) the General Public Licence v3 (GPL)
//   b) the Allors License
// 
// The GPL License is included in the file gpl.txt.
// The Allors License is an addendum to your contract.
// 
// Allors Platform is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU General Public License for more details.
// 
// For more information visit http://www.allors.com/legal
// </copyright>
// <summary>Defines the MediaTests type.</summary>
//-------------------------------------------------------------------------------------------------

namespace Allors.Database.Domain.Tests.Whist.Score
{
    using System.Linq;
    using Xunit;

    public class MiseryTests : DomainTest, IClassFixture<Fixture>
    {
        public MiseryTests(Fixture fixture) : base(fixture) { }

        private Scoreboard scoreboard;
        private Person player1;
        private Person player2;
        private Person player3;
        private Person player4;

        private GameModes GameModes;

        public void Setup(DerivationTypes data)
        {
            this.SelectDerivationType(data);

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
        public void TestSync(object data)
        {
            this.Setup((DerivationTypes)data);

            //Arrange
            var game = new GameBuilder(this.Transaction).Build();

            //Act
            scoreboard.AddGame(game);
            this.Transaction.Derive();

            //Assert
            Assert.Equal(4, game.Scores.Count());

        }

        [Theory]
        [MemberData(nameof(TestedDerivationTypes))]
        public void TestMiseryWithoutDeclarers(object data)
        {
            this.Setup((DerivationTypes)data);

            //Arrange
            var game = new GameBuilder(this.Transaction).Build();
            scoreboard.AddGame(game);

            //Act
            game.GameMode = this.GameModes.Misery;
            this.Transaction.Derive();

            //Assert
            Assert.Null(game.Scores.First(v => v.Player == player1).Value);
            Assert.Null(game.Scores.First(v => v.Player == player2).Value);
            Assert.Null(game.Scores.First(v => v.Player == player3).Value);
            Assert.Null(game.Scores.First(v => v.Player == player4).Value);
            Assert.True(this.scoreboard.ZeroTest());
        }

        [Theory]
        [MemberData(nameof(TestedDerivationTypes))]
        public void TestMiseryWithOneDeclarerAndOneWinner(object data)
        {
            this.Setup((DerivationTypes)data);

            //Arrange
            var game = new GameBuilder(this.Transaction).Build();
            scoreboard.AddGame(game);

            game.StartDate = this.Transaction.Now();
            game.EndDate = game.StartDate.Value.AddHours(1);

            //Act
            game.GameMode = this.GameModes.Misery;
            game.AddDeclarer(player1);
            game.AddWinner(this.player1);

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
        public void TestMiseryWithOneDeclarerAndZeroWinner(object data)
        {
            this.Setup((DerivationTypes)data);

            //Arrange
            var game = new GameBuilder(this.Transaction).Build();
            scoreboard.AddGame(game);

            game.StartDate = this.Transaction.Now();
            game.EndDate = game.StartDate.Value.AddHours(1);

            //Act
            game.GameMode = this.GameModes.Misery;
            game.AddDeclarer(player1);

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
        public void TestMiseryWithTwoDeclarersAndZeroWinners(object data)
        {
            this.Setup((DerivationTypes)data);

            //Arrange
            var game = new GameBuilder(this.Transaction).Build();
            scoreboard.AddGame(game);

            game.StartDate = this.Transaction.Now();
            game.EndDate = game.StartDate.Value.AddHours(1);

            //Act
            game.GameMode = this.GameModes.Misery;
            game.AddDeclarer(player1);
            game.AddDeclarer(player2);

            this.Transaction.Derive();

            //Assert
            Assert.Equal(-15, game.Scores.First(v => v.Player == player1).Value);
            Assert.Equal(-15, game.Scores.First(v => v.Player == player2).Value);
            Assert.Equal(15, game.Scores.First(v => v.Player == player3).Value);
            Assert.Equal(15, game.Scores.First(v => v.Player == player4).Value);
            Assert.True(this.scoreboard.ZeroTest());
        }

        [Theory]
        [MemberData(nameof(TestedDerivationTypes))]
        public void TestMiseryWithTwoDeclarersAndOneWinner(object data)
        {
            this.Setup((DerivationTypes)data);

            //Arrange
            var game = new GameBuilder(this.Transaction).Build();
            scoreboard.AddGame(game);

            game.StartDate = this.Transaction.Now();
            game.EndDate = game.StartDate.Value.AddHours(1);

            //Act
            game.GameMode = this.GameModes.Misery;
            game.AddDeclarer(player1);
            game.AddDeclarer(player2);

            game.AddWinner(player1);

            this.Transaction.Derive();

            //Assert
            Assert.Equal(15, game.Scores.First(v => v.Player == player1).Value);
            Assert.Equal(-15, game.Scores.First(v => v.Player == player2).Value);
            Assert.Equal(0, game.Scores.First(v => v.Player == player3).Value);
            Assert.Equal(0, game.Scores.First(v => v.Player == player4).Value);
            Assert.True(this.scoreboard.ZeroTest());
        }

        [Theory]
        [MemberData(nameof(TestedDerivationTypes))]
        public void TestMiseryWithTwoDeclarersAndTwoWinners(object data)
        {
            this.Setup((DerivationTypes)data);

            //Arrange
            var game = new GameBuilder(this.Transaction).Build();
            scoreboard.AddGame(game);

            game.StartDate = this.Transaction.Now();
            game.EndDate = game.StartDate.Value.AddHours(1);

            //Act
            game.GameMode = this.GameModes.Misery;
            game.AddDeclarer(player1);
            game.AddDeclarer(player2);

            game.AddWinner(player1);
            game.AddWinner(player2);

            this.Transaction.Derive();

            //Assert
            Assert.Equal(15, game.Scores.First(v => v.Player == player1).Value);
            Assert.Equal(15, game.Scores.First(v => v.Player == player2).Value);
            Assert.Equal(-15, game.Scores.First(v => v.Player == player3).Value);
            Assert.Equal(-15, game.Scores.First(v => v.Player == player4).Value);
            Assert.True(this.scoreboard.ZeroTest());
        }

        [Theory]
        [MemberData(nameof(TestedDerivationTypes))]
        public void TestMiseryWithFourDeclarersAndZeroWinners(object data)
        {
            this.Setup((DerivationTypes)data);

            //Arrange
            var game = new GameBuilder(this.Transaction).Build();
            scoreboard.AddGame(game);

            game.StartDate = this.Transaction.Now();
            game.EndDate = game.StartDate.Value.AddHours(1);

            //Act
            game.GameMode = this.GameModes.Misery;
            game.AddDeclarer(player1);
            game.AddDeclarer(player2);
            game.AddDeclarer(player3);
            game.AddDeclarer(player4);

            this.Transaction.Derive();

            //Assert
            Assert.Equal(0, game.Scores.First(v => v.Player == player1).Value);
            Assert.Equal(0, game.Scores.First(v => v.Player == player2).Value);
            Assert.Equal(-0, game.Scores.First(v => v.Player == player3).Value);
            Assert.Equal(-0, game.Scores.First(v => v.Player == player4).Value);
            Assert.True(this.scoreboard.ZeroTest());
        }

        [Theory]
        [MemberData(nameof(TestedDerivationTypes))]
        public void TestMiseryWithFourDeclarersAndOneWinner(object data)
        {
            this.Setup((DerivationTypes)data);

            //Arrange
            var game = new GameBuilder(this.Transaction).Build();
            scoreboard.AddGame(game);

            game.StartDate = this.Transaction.Now();
            game.EndDate = game.StartDate.Value.AddHours(1);

            //Act
            game.GameMode = this.GameModes.Misery;
            game.AddDeclarer(player1);
            game.AddDeclarer(player2);
            game.AddDeclarer(player3);
            game.AddDeclarer(player4);

            game.AddWinner(player1);

            this.Transaction.Derive();

            //Assert
            Assert.Equal(45, game.Scores.First(v => v.Player == player1).Value);
            Assert.Equal(-15, game.Scores.First(v => v.Player == player2).Value);
            Assert.Equal(-15, game.Scores.First(v => v.Player == player3).Value);
            Assert.Equal(-15, game.Scores.First(v => v.Player == player4).Value);
            Assert.True(this.scoreboard.ZeroTest());
        }

        [Theory]
        [MemberData(nameof(TestedDerivationTypes))]
        public void TestMiseryWithFourDeclarersAndTwoWinners(object data)
        {
            this.Setup((DerivationTypes)data);

            //Arrange
            var game = new GameBuilder(this.Transaction).Build();
            scoreboard.AddGame(game);

            game.StartDate = this.Transaction.Now();
            game.EndDate = game.StartDate.Value.AddHours(1);

            //Act
            game.GameMode = this.GameModes.Misery;
            game.AddDeclarer(player1);
            game.AddDeclarer(player2);
            game.AddDeclarer(player3);
            game.AddDeclarer(player4);

            game.AddWinner(player1);
            game.AddWinner(player2);

            this.Transaction.Derive();

            //Assert
            Assert.Equal(15, game.Scores.First(v => v.Player == player1).Value);
            Assert.Equal(15, game.Scores.First(v => v.Player == player2).Value);
            Assert.Equal(-15, game.Scores.First(v => v.Player == player3).Value);
            Assert.Equal(-15, game.Scores.First(v => v.Player == player4).Value);
            Assert.True(this.scoreboard.ZeroTest());
        }

        [Theory]
        [MemberData(nameof(TestedDerivationTypes))]
        public void TestMiseryWithFourDeclarersAndThreeWinners(object data)
        {
            this.Setup((DerivationTypes)data);

            //Arrange
            var game = new GameBuilder(this.Transaction).Build();
            scoreboard.AddGame(game);

            game.StartDate = this.Transaction.Now();
            game.EndDate = game.StartDate.Value.AddHours(1);

            //Act
            game.GameMode = this.GameModes.Misery;
            game.AddDeclarer(player1);
            game.AddDeclarer(player2);
            game.AddDeclarer(player3);
            game.AddDeclarer(player4);

            game.AddWinner(player1);
            game.AddWinner(player2);
            game.AddWinner(player3);

            this.Transaction.Derive();

            //Assert
            Assert.Equal(15, game.Scores.First(v => v.Player == player1).Value);
            Assert.Equal(15, game.Scores.First(v => v.Player == player2).Value);
            Assert.Equal(15, game.Scores.First(v => v.Player == player3).Value);
            Assert.Equal(-45, game.Scores.First(v => v.Player == player4).Value);
            Assert.True(this.scoreboard.ZeroTest());
        }
    }
}
