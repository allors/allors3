// <copyright file="Domain.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Database.Domain.Tests.Relation
{
    using Xunit;

    public class PersonTests : DomainTest, IClassFixture<Fixture>
    {
        public PersonTests(Fixture fixture) : base(fixture) { }

        [Fact]
        public void GivenPerson_WhenDeriving_ThenRequiredRelationsMustExist()
        {
            var builder = new PersonBuilder(this.Transaction);
            builder.Build();

            Assert.False(this.Transaction.Derive(false).HasErrors);
        }

        [Fact]
        public void OneDeclarerAndThreeDefenderDerivationTest()
        {
            var people = new People(this.Transaction);

            var player1 = people.FindBy(M.Person.UserName, "player1");
            var player2 = people.FindBy(M.Person.UserName, "player2");
            var player3 = people.FindBy(M.Person.UserName, "player3");
            var player4 = people.FindBy(M.Person.UserName, "player4");

            var scoreboard = new ScoreboardBuilder(this.Transaction)
                .WithPlayer(player1)
                .WithPlayer(player2)
                .WithPlayer(player3)
                .WithPlayer(player4)
                .Build();

            var GameTypes = new GameModes(this.Transaction);

            this.Transaction.Derive();

            var game = new GameBuilder(this.Transaction).Build();
            scoreboard.AddGame(game);

            game.StartDate = this.Transaction.Now();
            game.EndDate = game.StartDate.Value.AddHours(1);

            //Act
            game.GameMode = GameTypes.Solo;
            game.AddDeclarer(player1);

            this.Transaction.Derive();

            Assert.Contains(player1, game.Declarers);
            Assert.Contains(player2, game.Defenders);
            Assert.Contains(player3, game.Defenders);
            Assert.Contains(player4, game.Defenders);
        }

        [Fact]
        
        public void TwoDeclarerAndTwoDefenderDerivationTest()
        {
            var people = new People(this.Transaction);

            var player1 = people.FindBy(M.Person.UserName, "player1");
            var player2 = people.FindBy(M.Person.UserName, "player2");
            var player3 = people.FindBy(M.Person.UserName, "player3");
            var player4 = people.FindBy(M.Person.UserName, "player4");

            var scoreboard = new ScoreboardBuilder(this.Transaction)
                .WithPlayer(player1)
                .WithPlayer(player2)
                .WithPlayer(player3)
                .WithPlayer(player4)
                .Build();

            var GameTypes = new GameModes(this.Transaction);

            this.Transaction.Derive();

            var game = new GameBuilder(this.Transaction).Build();
            scoreboard.AddGame(game);

            game.StartDate = this.Transaction.Now();
            game.EndDate = game.StartDate.Value.AddHours(1);

            //Act
            game.GameMode = GameTypes.Misery;
            game.AddDeclarer(player1);
            game.AddDeclarer(player2);

            this.Transaction.Derive();

            Assert.Contains(player1, game.Declarers);
            Assert.Contains(player2, game.Declarers);
            Assert.Contains(player3, game.Defenders);
            Assert.Contains(player4, game.Defenders);
        }
    }
}
