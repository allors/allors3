// <copyright file="Domain.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Database.Domain.Tests.Whist
{
    using Xunit;

    public class GameTests : DomainTest, IClassFixture<Fixture>
    {
        private Scoreboard scoreboard;
        private Person player1;
        private Person player2;
        private Person player3;
        private Person player4;

        private GameModes GameTypes;

        public GameTests(Fixture fixture) : base(fixture)
        {
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

            this.GameTypes = new GameModes(this.Transaction);

            this.Transaction.Derive();
        }

        [Fact]
        public void TestStartDateBeforeEndDate()
        {
            // Arrange
            var game = new GameBuilder(this.Transaction).Build();
            this.scoreboard.AddGame(game);

            this.Transaction.Derive();

            // Act
            game.StartDate = this.Transaction.Now();
            game.EndDate = game.StartDate.Value.AddHours(-1);

            var validation = this.Transaction.Derive(false);

            // Assert
            Assert.True(validation.HasErrors);
        }

        [Fact]
        public void TestStartDateDoesNotEqualsEndDate()
        {
            // Arrange
            var game = new GameBuilder(this.Transaction).Build();
            this.scoreboard.AddGame(game);

            this.Transaction.Derive();

            // Act
            game.StartDate = this.Transaction.Now();
            game.EndDate = game.StartDate;

            var validation = this.Transaction.Derive(false);

            // Assert
            Assert.True(validation.HasErrors);
        }
    }
}
