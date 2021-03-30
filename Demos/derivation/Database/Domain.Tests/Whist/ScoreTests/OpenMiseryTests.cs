// <copyright file="Domain.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Database.Domain.Tests.Whist.Score
{
    using System.Linq;
    using Xunit;

    public class OpenMiseryTests : DomainTest, IClassFixture<Fixture>
    {
        public OpenMiseryTests(Fixture fixture) : base(fixture) { }

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
        public void TesOpenMiseryWithOneDeclarerAndOneWinner(object data)
        {
            this.Setup((DerivationTypes)data);

            //Arrange
            var game = new GameBuilder(this.Transaction).Build();
            scoreboard.AddGame(game);

            game.StartDate = this.Transaction.Now();
            game.EndDate = game.StartDate.Value.AddHours(1);

            //Act
            game.GameMode = this.GameModes.OpenMisery;
            game.AddDeclarer(player1);
            game.AddWinner(this.player1);

            this.Transaction.Derive();

            //Assert
            Assert.Equal(30, game.Scores.First(v => v.Player == player1).Value);
            Assert.Equal(-10, game.Scores.First(v => v.Player == player2).Value);
            Assert.Equal(-10, game.Scores.First(v => v.Player == player3).Value);
            Assert.Equal(-10, game.Scores.First(v => v.Player == player4).Value);
            Assert.True(this.scoreboard.ZeroTest());
        }
    }
}
