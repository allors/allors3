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

namespace Allors.Domain
{
    using Allors.Meta;
    using System.Linq;
    using Xunit;

    public class MisèreTests : DomainTest
    {
        private Scoreboard scoreboard;
        private Person player1;
        private Person player2;
        private Person player3;
        private Person player4;

        private GameModes GameModes;

        public void Setup(DerivationTypes data)
        {
            this.RegisterDerivations(data);

            var people = new People(this.Session);

            this.player1 = people.FindBy(M.Person.UserName, "speler1");
            this.player2 = people.FindBy(M.Person.UserName, "speler2");
            this.player3 = people.FindBy(M.Person.UserName, "speler3");
            this.player4 = people.FindBy(M.Person.UserName, "speler4");

            this.scoreboard = new ScoreboardBuilder(this.Session)
                .WithPlayer(player1)
                .WithPlayer(player2)
                .WithPlayer(player3)
                .WithPlayer(player4)
                .Build();

            this.GameModes = new GameModes(this.Session);

            this.Session.Derive();
        }

        [Theory]
        [MemberData(nameof(TestedDerivationTypes))]
        public void TestSync(object data)
        {
            this.Setup((DerivationTypes)data);

            //Arrange
            var game = new GameBuilder(this.Session).Build();

            //Act
            scoreboard.AddGame(game);
            this.Session.Derive();

            //Assert
            Assert.Equal(4, game.Scores.Count);

        }

        [Theory]
        [MemberData(nameof(TestedDerivationTypes))]
        public void TestMisèreWithoutDeclarers(object data)
        {
            this.Setup((DerivationTypes)data);

            //Arrange
            var game = new GameBuilder(this.Session).Build();
            scoreboard.AddGame(game);

            //Act
            game.GameMode = this.GameModes.Misère;
            this.Session.Derive();

            //Assert
            Assert.Null(game.Scores.First(v => v.Player == player1).Value);
            Assert.Null(game.Scores.First(v => v.Player == player2).Value);
            Assert.Null(game.Scores.First(v => v.Player == player3).Value);
            Assert.Null(game.Scores.First(v => v.Player == player4).Value);
            Assert.True(this.scoreboard.NulProef());
        }

        [Theory]
        [MemberData(nameof(TestedDerivationTypes))]
        public void TestMisèreWithOneDeclarerAndOneWinner(object data)
        {
            this.Setup((DerivationTypes)data);

            //Arrange
            var game = new GameBuilder(this.Session).Build();
            scoreboard.AddGame(game);

            game.StartDate = this.Session.Now();
            game.EndDate = game.StartDate.Value.AddHours(1);

            //Act
            game.GameMode = this.GameModes.Misère;
            game.AddDeclarer(player1);
            game.AddWinner(this.player1);

            this.Session.Derive();

            //Assert
            Assert.Equal(15, game.Scores.First(v => v.Player == player1).Value);
            Assert.Equal(-5, game.Scores.First(v => v.Player == player2).Value);
            Assert.Equal(-5, game.Scores.First(v => v.Player == player3).Value);
            Assert.Equal(-5, game.Scores.First(v => v.Player == player4).Value);
            Assert.True(this.scoreboard.NulProef());
        }

        [Theory]
        [MemberData(nameof(TestedDerivationTypes))]
        public void TestMisèreWithOneDeclarerAndZeroWinner(object data)
        {
            this.Setup((DerivationTypes)data);

            //Arrange
            var game = new GameBuilder(this.Session).Build();
            scoreboard.AddGame(game);

            game.StartDate = this.Session.Now();
            game.EndDate = game.StartDate.Value.AddHours(1);

            //Act
            game.GameMode = this.GameModes.Misère;
            game.AddDeclarer(player1);

            this.Session.Derive();

            //Assert
            Assert.Equal(-15, game.Scores.First(v => v.Player == player1).Value);
            Assert.Equal(5, game.Scores.First(v => v.Player == player2).Value);
            Assert.Equal(5, game.Scores.First(v => v.Player == player3).Value);
            Assert.Equal(5, game.Scores.First(v => v.Player == player4).Value);
            Assert.True(this.scoreboard.NulProef());
        }

        [Theory]
        [MemberData(nameof(TestedDerivationTypes))]
        public void TestMisèreWithTwoDeclarersAndZeroWinners(object data)
        {
            this.Setup((DerivationTypes)data);

            //Arrange
            var game = new GameBuilder(this.Session).Build();
            scoreboard.AddGame(game);

            game.StartDate = this.Session.Now();
            game.EndDate = game.StartDate.Value.AddHours(1);

            //Act
            game.GameMode = this.GameModes.Misère;
            game.AddDeclarer(player1);
            game.AddDeclarer(player2);

            this.Session.Derive();

            //Assert
            Assert.Equal(-15, game.Scores.First(v => v.Player == player1).Value);
            Assert.Equal(-15, game.Scores.First(v => v.Player == player2).Value);
            Assert.Equal(15, game.Scores.First(v => v.Player == player3).Value);
            Assert.Equal(15, game.Scores.First(v => v.Player == player4).Value);
            Assert.True(this.scoreboard.NulProef());
        }

        [Theory]
        [MemberData(nameof(TestedDerivationTypes))]
        public void TestMisèreWithTwoDeclarersAndOneWinner(object data)
        {
            this.Setup((DerivationTypes)data);

            //Arrange
            var game = new GameBuilder(this.Session).Build();
            scoreboard.AddGame(game);

            game.StartDate = this.Session.Now();
            game.EndDate = game.StartDate.Value.AddHours(1);

            //Act
            game.GameMode = this.GameModes.Misère;
            game.AddDeclarer(player1);
            game.AddDeclarer(player2);

            game.AddWinner(player1);

            this.Session.Derive();

            //Assert
            Assert.Equal(15, game.Scores.First(v => v.Player == player1).Value);
            Assert.Equal(-15, game.Scores.First(v => v.Player == player2).Value);
            Assert.Equal(0, game.Scores.First(v => v.Player == player3).Value);
            Assert.Equal(0, game.Scores.First(v => v.Player == player4).Value);
            Assert.True(this.scoreboard.NulProef());
        }

        [Theory]
        [MemberData(nameof(TestedDerivationTypes))]
        public void TestMisèreWithTwoDeclarersAndTwoWinners(object data)
        {
            this.Setup((DerivationTypes)data);

            //Arrange
            var game = new GameBuilder(this.Session).Build();
            scoreboard.AddGame(game);

            game.StartDate = this.Session.Now();
            game.EndDate = game.StartDate.Value.AddHours(1);

            //Act
            game.GameMode = this.GameModes.Misère;
            game.AddDeclarer(player1);
            game.AddDeclarer(player2);

            game.AddWinner(player1);
            game.AddWinner(player2);

            this.Session.Derive();

            //Assert
            Assert.Equal(15, game.Scores.First(v => v.Player == player1).Value);
            Assert.Equal(15, game.Scores.First(v => v.Player == player2).Value);
            Assert.Equal(-15, game.Scores.First(v => v.Player == player3).Value);
            Assert.Equal(-15, game.Scores.First(v => v.Player == player4).Value);
            Assert.True(this.scoreboard.NulProef());
        }

        [Theory]
        [MemberData(nameof(TestedDerivationTypes))]
        public void TestMisèreWithFourDeclarersAndZeroWinners(object data)
        {
            this.Setup((DerivationTypes)data);

            //Arrange
            var game = new GameBuilder(this.Session).Build();
            scoreboard.AddGame(game);

            game.StartDate = this.Session.Now();
            game.EndDate = game.StartDate.Value.AddHours(1);

            //Act
            game.GameMode = this.GameModes.Misère;
            game.AddDeclarer(player1);
            game.AddDeclarer(player2);
            game.AddDeclarer(player3);
            game.AddDeclarer(player4);

            this.Session.Derive();

            //Assert
            Assert.Equal(0, game.Scores.First(v => v.Player == player1).Value);
            Assert.Equal(0, game.Scores.First(v => v.Player == player2).Value);
            Assert.Equal(-0, game.Scores.First(v => v.Player == player3).Value);
            Assert.Equal(-0, game.Scores.First(v => v.Player == player4).Value);
            Assert.True(this.scoreboard.NulProef());
        }

        [Theory]
        [MemberData(nameof(TestedDerivationTypes))]
        public void TestMisèreWithFourDeclarersAndOneWinner(object data)
        {
            this.Setup((DerivationTypes)data);

            //Arrange
            var game = new GameBuilder(this.Session).Build();
            scoreboard.AddGame(game);

            game.StartDate = this.Session.Now();
            game.EndDate = game.StartDate.Value.AddHours(1);

            //Act
            game.GameMode = this.GameModes.Misère;
            game.AddDeclarer(player1);
            game.AddDeclarer(player2);
            game.AddDeclarer(player3);
            game.AddDeclarer(player4);

            game.AddWinner(player1);

            this.Session.Derive();

            //Assert
            Assert.Equal(45, game.Scores.First(v => v.Player == player1).Value);
            Assert.Equal(-15, game.Scores.First(v => v.Player == player2).Value);
            Assert.Equal(-15, game.Scores.First(v => v.Player == player3).Value);
            Assert.Equal(-15, game.Scores.First(v => v.Player == player4).Value);
            Assert.True(this.scoreboard.NulProef());
        }

        [Theory]
        [MemberData(nameof(TestedDerivationTypes))]
        public void TestMisèreWithFourDeclarersAndTwoWinners(object data)
        {
            this.Setup((DerivationTypes)data);

            //Arrange
            var game = new GameBuilder(this.Session).Build();
            scoreboard.AddGame(game);

            game.StartDate = this.Session.Now();
            game.EndDate = game.StartDate.Value.AddHours(1);

            //Act
            game.GameMode = this.GameModes.Misère;
            game.AddDeclarer(player1);
            game.AddDeclarer(player2);
            game.AddDeclarer(player3);
            game.AddDeclarer(player4);

            game.AddWinner(player1);
            game.AddWinner(player2);

            this.Session.Derive();

            //Assert
            Assert.Equal(15, game.Scores.First(v => v.Player == player1).Value);
            Assert.Equal(15, game.Scores.First(v => v.Player == player2).Value);
            Assert.Equal(-15, game.Scores.First(v => v.Player == player3).Value);
            Assert.Equal(-15, game.Scores.First(v => v.Player == player4).Value);
            Assert.True(this.scoreboard.NulProef());
        }

        [Theory]
        [MemberData(nameof(TestedDerivationTypes))]
        public void TestMisèreWithFourDeclarersAndThreeWinners(object data)
        {
            this.Setup((DerivationTypes)data);

            //Arrange
            var game = new GameBuilder(this.Session).Build();
            scoreboard.AddGame(game);

            game.StartDate = this.Session.Now();
            game.EndDate = game.StartDate.Value.AddHours(1);

            //Act
            game.GameMode = this.GameModes.Misère;
            game.AddDeclarer(player1);
            game.AddDeclarer(player2);
            game.AddDeclarer(player3);
            game.AddDeclarer(player4);

            game.AddWinner(player1);
            game.AddWinner(player2);
            game.AddWinner(player3);

            this.Session.Derive();

            //Assert
            Assert.Equal(15, game.Scores.First(v => v.Player == player1).Value);
            Assert.Equal(15, game.Scores.First(v => v.Player == player2).Value);
            Assert.Equal(15, game.Scores.First(v => v.Player == player3).Value);
            Assert.Equal(-45, game.Scores.First(v => v.Player == player4).Value);
            Assert.True(this.scoreboard.NulProef());
        }
    }
}
